using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Interfaces;
using FitnessAnalyticsHub.Infrastructure.Configuration;
using FitnessAnalyticsHub.Infrastructure.Extensions;
using FitnessAnalyticsHub.Infrastructure.Persistence;
using FitnessAnalyticsHub.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessAnalyticsHub.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration
        services.AddDbContext<ApplicationDbContext>(options =>
                DatabaseConfiguration.ConfigureDatabase(options, configuration));

        // Register DbContext Interface
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Register Strava service
        services.AddScoped<IStravaService, StravaService>();

        // HTTP Client for Strava API
        services.AddHttpClient("StravaApi", client =>
        {
            client.BaseAddress = new Uri("https://www.strava.com/api/v3/");
        });

        // Service registrieren
        services.AddScoped<IAIAssistantClientService, AIAssistantClientService>();

        // HttpClient für AIAssistant registrieren
        services.AddHttpClient<IAIAssistantClientService, AIAssistantClientService>(client =>
        {
            client.BaseAddress = new Uri(configuration["AIAssistant:BaseUrl"] ?? "http://localhost:5169");
        });

        // HealthChecks für die Infrastruktur hinzufügen
        services.AddHealthChecks()
            .AddInfrastructureHealthChecks(configuration);

        return services;
    }
}
