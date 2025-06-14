using System.Reflection;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Application.Mapping;
using FitnessAnalyticsHub.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessAnalyticsHub.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registriere AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddTransient<MappingProfile>();

        // Registriere Services
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IAthleteService, AthleteService>();
        services.AddScoped<ITrainingPlanService, TrainingPlanService>();

        return services;
    }
}
