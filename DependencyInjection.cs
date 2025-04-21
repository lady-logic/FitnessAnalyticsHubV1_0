using System;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IStravaService, StravaService>();

        // HTTP Client for Strava API
        services.AddHttpClient("StravaApi", client =>
        {
            client.BaseAddress = new Uri("https://www.strava.com/api/v3/");
        });

        return services;
    }
}
