using System.Reflection;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Accessors.DependencyInjection;
using VoiceProcessor.Clients.Api.Authentication;
using VoiceProcessor.Clients.Api.Hubs;
using VoiceProcessor.Clients.Api.Notifications;
using VoiceProcessor.Clients.Api.Services;
using VoiceProcessor.Domain.Contracts.Accessors;
using VoiceProcessor.Engines.DependencyInjection;
using VoiceProcessor.Managers.Contracts;
using VoiceProcessor.Managers.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Controllers with JSON enum string serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Memory cache for caching (used by StripeAccessor)
builder.Services.AddMemoryCache();

// Database
builder.Services.AddDbContext<VoiceProcessorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Accessors (database, TTS providers, storage)
builder.Services.AddAccessors(builder.Configuration);

// Engines (routing, chunking, pricing)
builder.Services.AddEngines(builder.Configuration);

// Managers (user, voice, generation)
builder.Services.AddManagers(builder.Configuration);

// SignalR real-time notifications
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationAccessor, GenerationNotificationAccessor>();

// Authentication
builder.Services.AddVoiceProcessorAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Background services
builder.Services.AddHostedService<VoiceSeedingService>();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Hangfire for background jobs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string 'ConnectionStrings:DefaultConnection' is not configured. " +
        "Please set the ConnectionStrings__DefaultConnection environment variable.");
}

// Convert PostgreSQL URI format to ADO.NET connection string format if needed
// Railway provides: postgresql://user:pass@host:port/db
// Hangfire needs: Host=host;Port=port;Database=db;Username=user;Password=pass
if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]}";
    Console.WriteLine($"[INFO] Converted PostgreSQL URI to ADO.NET format");
}

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
    options.Queues = new[] { "default", "generation" };
});

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "VoiceProcessor API",
        Description = "Multi-provider Text-to-Speech SaaS API",
        Contact = new OpenApiContact
        {
            Name = "VoiceProcessor Team"
        }
    });

    // JWT Bearer authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // API Key authentication
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication. Enter your API key (starts with 'vp_').",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    // Security requirements
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });

    // Include XML comments for API documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "VoiceProcessor API v1");
        options.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GenerationHub>("/hubs/generation");

// Hangfire dashboard (development only for now)
if (app.Environment.IsDevelopment())
{
    app.MapHangfireDashboard("/hangfire");
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

// Configure recurring jobs after app is built
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<IVoiceManager>(
    "refresh-voice-catalog",
    manager => manager.RefreshVoiceCatalogAsync(CancellationToken.None),
    Cron.Daily(3)); // Run at 3 AM daily

app.Run();
