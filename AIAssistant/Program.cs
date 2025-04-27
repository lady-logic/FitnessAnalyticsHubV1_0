using AIAssistant._02_Application.Interfaces;
using AIAssistant._03_Infrastructure.OpenAI.Models;
using AIAssistant._03_Infrastructure.Services;
using FitnessAnalyticsHub.AIAssistant._03_Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// OpenAI Konfiguration
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));

// Service-Registrierungen
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAIPromptService, LLMService>();
builder.Services.AddScoped<IWorkoutAnalysisService, WorkoutAnalysisService>();
builder.Services.AddScoped<IMotivationCoachService, MotivationCoachService>();
builder.Services.AddScoped<IWorkoutPredictionService, WorkoutPredictionService>();

// Controller hinzufügen
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddEnvironmentVariables();

// CORS konfigurieren
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFitnessApp", builder =>
    {
        builder.WithOrigins("http://localhost:5000") // URL deiner Hauptanwendung
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

// API-Schlüssel überprüfen
if (string.IsNullOrEmpty(app.Configuration["OpenAI:ApiKey"]))
{
    app.Logger.LogError("OpenAI API-Key fehlt! Der Service wird nicht korrekt funktionieren.");
    if (app.Environment.IsDevelopment())
    {
        app.Logger.LogError("Führe 'dotnet user-secrets set \"OpenAI:ApiKey\" \"dein-api-key\"' aus, um den Schlüssel einzurichten.");
    }
    else if (app.Environment.IsProduction())
    {
        app.Logger.LogError("Stelle sicher, dass die Umgebungsvariable OPENAI__APIKEY gesetzt ist.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFitnessApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
