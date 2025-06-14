using AIAssistant._02_Application.Interfaces;
using AIAssistant._03_Infrastructure.Services;
using FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Fitness Analytics Hub - AI Assistant",
        Version = "v1",
        Description = "AI-powered fitness analytics"
    });
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// HTTP Clients registrieren
builder.Services.AddHttpClient<HuggingFaceService>();
builder.Services.AddHttpClient<GoogleGeminiService>();

builder.Services.AddScoped<IAIPromptService>(provider =>
{
    var defaultProvider = builder.Configuration["AI:DefaultProvider"] ?? "GoogleGemini";
    return defaultProvider.ToLower() switch
    {
        "huggingface" => provider.GetRequiredService<HuggingFaceService>(),
        "googlegemini" => provider.GetRequiredService<GoogleGeminiService>(),
        _ => provider.GetRequiredService<GoogleGeminiService>()
    };
});
builder.Services.AddScoped<HuggingFaceService>();
builder.Services.AddScoped<GoogleGeminiService>();

// Application Services registrieren
builder.Services.AddScoped<IMotivationCoachService, MotivationCoachService>();
builder.Services.AddScoped<IWorkoutAnalysisService, WorkoutAnalysisService>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Assistant API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

app.UseHttpsRedirection();

// CORS aktivieren
app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new
{
    status = "healthy",
    service = "AI Assistant",
    timestamp = DateTime.UtcNow
});

Console.WriteLine("🤖 AI Assistant Service starting...");
Console.WriteLine("📊 Using HuggingFace / GoogleGemini for AI processing");
Console.WriteLine("🌐 Swagger UI available at: https://localhost:7276");

app.Run();