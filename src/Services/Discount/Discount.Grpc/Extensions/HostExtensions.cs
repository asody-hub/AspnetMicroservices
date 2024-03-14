﻿using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Discount.Grpc.Extensions;

public static class HostExtensions
{
    public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
    {
        int retryForAvailability = retry ?? 0;

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                logger.LogInformation("Migrating PostgreSQL database.");
                
                using var connection = new NpgsqlConnection
                    (configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
                connection.Open();

                using var command = connection.CreateCommand();

                //command.CommandText = "DROP TABLE IF EXISTS Coupon";
                //command.ExecuteNonQuery();

                command.CommandText = @"SELECT EXISTS (SELECT FROM pg_catalog.pg_class c WHERE c.relname ilike 'Coupon');";
                var isTableExists = command.ExecuteScalar();

                if (isTableExists != null && (bool)isTableExists)
                {
                    logger.LogInformation("No need in PostgreSQL database migration.");
                    return host;
                }

                command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY, 
                                                                ProductName VARCHAR(24) NOT NULL,
                                                                Description TEXT,
                                                                Amount INT)";
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
                command.ExecuteNonQuery();

                logger.LogInformation("Migrated PostgreSQL database.");
            }
            catch (NpgsqlException ex)
            {
                logger.LogError(ex, "An error occurred while migrating the PostgreSQL database.");

                if (retryForAvailability < 50)
                {
                    retryForAvailability++;
                    System.Threading.Thread.Sleep(2000);
                    MigrateDatabase<TContext>(host, retryForAvailability);
                }
            }
        }

        return host;
    }
}
