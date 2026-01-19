using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Accessors.DependencyInjection;
using VoiceProcessor.Clients.Api.Authentication;
using VoiceProcessor.Clients.Api.Services;
using VoiceProcessor.Engines.DependencyInjection;
using VoiceProcessor.Managers.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Controllers with JSON enum string serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Database
builder.Services.AddDbContext<VoiceProcessorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Accessors (database, TTS providers, storage)
builder.Services.AddAccessors(builder.Configuration);

// Engines (routing, chunking, pricing)
builder.Services.AddEngines(builder.Configuration);

// Managers (user, voice, generation)
builder.Services.AddManagers(builder.Configuration);

// Authentication
builder.Services.AddVoiceProcessorAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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

    // Security requirements - function that returns the requirement from the document
    options.AddSecurityRequirement(document =>
    {
        var requirement = new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() },
            { new OpenApiSecuritySchemeReference("ApiKey", document), new List<string>() }
        };
        return requirement;
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();
