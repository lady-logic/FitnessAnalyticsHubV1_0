using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessAnalyticsHub.Infrastructure.Extensions
{
    // In einer Klasse wie InfrastructureHealthChecks.cs
    public static class InfrastructureHealthChecksExtension
    {
        public static IHealthChecksBuilder AddInfrastructureHealthChecks(
            this IHealthChecksBuilder builder,
            IConfiguration configuration)
        {
            // Datenbank-Checks
            builder.AddSqlServer(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                name: "database",
                tags: new[] { "db", "sql", "infrastructure" });

            // Redis-Cache Check, falls verwendet
            if (!string.IsNullOrEmpty(configuration["Redis:ConnectionString"]))
            {
                builder.AddRedis(
                    redisConnectionString: configuration["Redis:ConnectionString"],
                    name: "redis-cache",
                    tags: new[] { "cache", "infrastructure" });
            }

            // Weitere Infrastruktur-Checks...

            return builder;
        }
    }
}
