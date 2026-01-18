using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Accessors.Data.DbContext;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<VoiceProcessorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// OpenAPI/Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();
