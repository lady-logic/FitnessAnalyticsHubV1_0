using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Interfaces;
using FitnessAnalyticsHub.Infrastructure.Extensions;
using FitnessAnalyticsHub.Infrastructure.Persistence;
using FitnessAnalyticsHub.Infrastructure.Repositories;
using FitnessAnalyticsHub.Infrastructure.Services;
using FitnessAnalyticsHub.Infrastructure.Services.AIAssistant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessAnalyticsHub.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database configuration
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    configuration.GetConnectionString("DefaultConnection")));

            // Register repositories
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            // Configure Strava service
            services.Configure<StravaSettings>(configuration.GetSection("Strava"));
            services.AddSingleton(serviceProvider =>
            {
                var config = configuration.GetSection("Strava");
                return new StravaSettings
                {
                    ClientId = config["ClientId"],
                    ClientSecret = config["ClientSecret"],
                    RedirectUrl = config["RedirectUrl"]
                };
            });

            // Register Strava service
            services.AddScoped<IStravaService, StravaService>();

            // HTTP Client for Strava API
            services.AddHttpClient("StravaApi", client =>
            {
                client.BaseAddress = new Uri("https://www.strava.com/api/v3/");
            });

            // HttpClient für AIAssistant registrieren
            services.AddHttpClient<IAIAssistantClient, AIAssistantClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["AIAssistant:BaseUrl"]);
            });

            // HealthChecks für die Infrastruktur hinzufügen
            services.AddHealthChecks()
                .AddInfrastructureHealthChecks(configuration);

            return services;
        }
    }
}
