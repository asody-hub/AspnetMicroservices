using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ordering.API.Extensions;

public static class HostExtensions
{
    public static IHost MigrateDatabase<TContext>(this IHost host, 
                                                Action<TContext, IServiceProvider> seeder, 
                                                int? retry = 0) 
        where TContext : DbContext
    {
        int retryForAvailability = retry ?? 0;

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetService<TContext>();
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}.", nameof(context));

                InvokeSeeder(seeder, context, services);

                logger.LogInformation("Migrated database associated with context {DbContextName}.", typeof(TContext).Name);
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database associated with context {DbContextName}.", typeof(TContext).Name);

                if (retryForAvailability < 50)
                {
                    retryForAvailability++;
                    System.Threading.Thread.Sleep(2000);
                    MigrateDatabase<TContext>(host, seeder, retryForAvailability);
                }
            }
        }

        return host;
    }

    private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder,
                                                TContext context,
                                                IServiceProvider services)
        where TContext : DbContext
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            seeder(context, services);
        };
    }

}
