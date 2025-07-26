using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FitnessAnalyticsHub.Infrastructure.Configuration;

public static class DatabaseConfiguration
{
    public static void ConfigureDatabase(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        string provider = configuration.GetValue<string>("Database:Provider", "Sqlite");

        switch (provider.ToLower())
        {
            case "sqlite":
                ConfigureSqlite(options, connectionString ?? "Data Source=FitnessAnalytics.db");
                break;
            case "sqlserver":
                options.UseSqlServer(connectionString);
                break;
            default:
                ConfigureSqlite(options, connectionString ?? "Data Source=FitnessAnalytics.db");
                break;
        }

        ConfigureCommonOptions(options, configuration);
    }

    public static void ConfigureDatabase(DbContextOptionsBuilder options, string connectionString)
    {
        ConfigureSqlite(options, connectionString);

        // Design-Time Defaults
        options.EnableSensitiveDataLogging(true);
        options.EnableDetailedErrors(true);
    }

    private static void ConfigureSqlite(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseSqlite(connectionString);
    }

    private static void ConfigureCommonOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        bool isDevelopment = configuration.GetValue<bool>("Environment:IsDevelopment", false);

        options.EnableSensitiveDataLogging(isDevelopment);
        options.EnableDetailedErrors(isDevelopment);
    }
}