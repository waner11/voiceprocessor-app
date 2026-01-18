using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using VoiceProcessor.Accessors.Data.DbContext;
using VoiceProcessor.Accessors.DependencyInjection;
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

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();
