using FitnessAnalyticsHub.Application;
using FitnessAnalyticsHub.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fitness Analytics Hub API",
        Version = "v1",
        Description = "Backend API f�r die FitnessAnalyticsHub-Anwendung"
    });
});

// Register application services
builder.Services.AddApplication();

// Register infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Base HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck("api", () => HealthCheckResult.Healthy(), tags: new[] { "service" });

// UI f�r HealthChecks
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(60); // Alle 60 Sekunden pr�fen
    setup.MaximumHistoryEntriesPerEndpoint(50); // 50 Eintr�ge in der Historie speichern
})
.AddInMemoryStorage(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fitness Analytics Hub API v1");
    c.RoutePrefix = string.Empty; // Um Swagger als Startseite zu setzen
});

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

// HealthChecks Endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Gruppierte HealthChecks nach Tags
app.MapHealthChecks("/health/infrastructure", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("infrastructure"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Health UI Dashboard
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

app.MapControllers();

app.Run();
