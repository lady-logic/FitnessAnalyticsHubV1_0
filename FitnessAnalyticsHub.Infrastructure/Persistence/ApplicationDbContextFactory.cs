using FitnessAnalyticsHub.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FitnessAnalyticsHub.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        DatabaseConfiguration.ConfigureDatabase(optionsBuilder, "Data Source=FitnessAnalytics.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
