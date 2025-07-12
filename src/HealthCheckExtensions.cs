using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HealthChecks.UI.Client;

namespace FastTechFoods.Observability
{
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Configures comprehensive FastTechFoods HealthChecks using configuration values from the 'Observability' section.
        /// </summary>
        /// <typeparam name="TDbContext">The Entity Framework DbContext type for database health checks</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configuration">The configuration object to read from</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFastTechFoodsHealthChecks<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TDbContext : DbContext
        {
            // Get observability configuration section
            var observabilityConfig = configuration.GetSection("Observability");
            var serviceName = observabilityConfig["ServiceName"] ?? "FastTechFoods.Service";

            // Configure HealthChecks
            services.AddHealthChecks()
                .AddDbContextCheck<TDbContext>("database-context");

            // Configure HealthChecks UI with in-memory storage
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(15);
                setup.MaximumHistoryEntriesPerEndpoint(60);
                setup.AddHealthCheckEndpoint(serviceName, "/health");
            })
            .AddInMemoryStorage();

            return services;
        }

        /// <summary>
        /// Configures comprehensive FastTechFoods HealthChecks with custom database health check.
        /// </summary>
        /// <typeparam name="TDbContext">The Entity Framework DbContext type for database health checks</typeparam>
        /// <typeparam name="THealthCheck">The custom database health check implementation</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configuration">The configuration object to read from</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFastTechFoodsHealthChecks<TDbContext, THealthCheck>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TDbContext : DbContext
            where THealthCheck : class, Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
        {
            // Get observability configuration section
            var observabilityConfig = configuration.GetSection("Observability");
            var serviceName = observabilityConfig["ServiceName"] ?? "FastTechFoods.Service";

            // Configure HealthChecks with custom health check (without duplicating DbContext check)
            services.AddHealthChecks()
                .AddDbContextCheck<TDbContext>("database-context")
                .AddCheck<THealthCheck>("custom-health-check");

            // Configure HealthChecks UI with in-memory storage
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(15);
                setup.MaximumHistoryEntriesPerEndpoint(60);
                setup.AddHealthCheckEndpoint(serviceName, "/health");
            })
            .AddInMemoryStorage();

            return services;
        }

        /// <summary>
        /// Configures only HealthChecks without duplicating any observability configuration.
        /// Use this when you already have observability configured and only need health checks.
        /// </summary>
        /// <typeparam name="TDbContext">The Entity Framework DbContext type for database health checks</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="serviceName">The service name for health check UI</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFastTechFoodsHealthChecksOnly<TDbContext>(
            this IServiceCollection services,
            string serviceName = "FastTechFoods.Service")
            where TDbContext : DbContext
        {
            // Configure only HealthChecks
            services.AddHealthChecks()
                .AddDbContextCheck<TDbContext>("database-context");

            // Configure HealthChecks UI with in-memory storage
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(15);
                setup.MaximumHistoryEntriesPerEndpoint(60);
                setup.AddHealthCheckEndpoint(serviceName, "/health");
            })
            .AddInMemoryStorage();

            return services;
        }

        /// <summary>
        /// Configures only HealthChecks with custom health check without duplicating any observability configuration.
        /// Use this when you already have observability configured and only need health checks.
        /// </summary>
        /// <typeparam name="TDbContext">The Entity Framework DbContext type for database health checks</typeparam>
        /// <typeparam name="THealthCheck">The custom database health check implementation</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="serviceName">The service name for health check UI</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFastTechFoodsHealthChecksOnly<TDbContext, THealthCheck>(
            this IServiceCollection services,
            string serviceName = "FastTechFoods.Service")
            where TDbContext : DbContext
            where THealthCheck : class, Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
        {
            // Configure HealthChecks with custom health check
            services.AddHealthChecks()
                .AddDbContextCheck<TDbContext>("database-context")
                .AddCheck<THealthCheck>("custom-health-check");

            // Configure HealthChecks UI with in-memory storage
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(15);
                setup.MaximumHistoryEntriesPerEndpoint(60);
                setup.AddHealthCheckEndpoint(serviceName, "/health");
            })
            .AddInMemoryStorage();

            return services;
        }

        /// <summary>
        /// Configures the HealthChecks UI middleware for FastTechFoods applications.
        /// This should be called in the Configure method of your Startup class or in Program.cs.
        /// </summary>
        /// <param name="app">The IApplicationBuilder to configure</param>
        /// <returns>The IApplicationBuilder for chaining</returns>
        public static IApplicationBuilder UseFastTechFoodsHealthChecksUI(this IApplicationBuilder app)
        {
            // Configure health check endpoints
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // Configure HealthChecks UI
            app.UseHealthChecksUI(setup =>
            {
                setup.UIPath = "/health-ui";
                setup.ApiPath = "/health-ui-api";
            });

            return app;
        }

        /// <summary>
        /// Configures FastTechFoods HealthChecks with MongoDB support.
        /// Note: You need to install the AspNetCore.HealthChecks.MongoDb package first.
        /// Example: Install-Package AspNetCore.HealthChecks.MongoDb
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configuration">The configuration object to read from</param>
        /// <param name="mongoConnectionString">MongoDB connection string</param>
        /// <param name="mongoDatabaseName">MongoDB database name (optional)</param>
        /// <returns>The IServiceCollection for chaining</returns>
        /// <remarks>
        /// This method requires the AspNetCore.HealthChecks.MongoDb package to be installed in your project.
        /// The MongoDB client is registered as a singleton as recommended by MongoDB.
        /// </remarks>
        public static IServiceCollection AddFastTechFoodsHealthChecksWithMongoDB(
            this IServiceCollection services,
            IConfiguration configuration,
            string mongoConnectionString,
            string? mongoDatabaseName = null)
        {
            // Get observability configuration section
            var observabilityConfig = configuration.GetSection("Observability");
            var serviceName = observabilityConfig["ServiceName"] ?? "FastTechFoods.Service";

            // Register MongoClient as singleton (MongoDB recommendation)
            services.AddSingleton(sp => new MongoDB.Driver.MongoClient(mongoConnectionString));

            // Configure HealthChecks with MongoDB
            var healthChecksBuilder = services.AddHealthChecks();
            
            if (!string.IsNullOrEmpty(mongoDatabaseName))
            {
                // Check specific database
                healthChecksBuilder.AddMongoDb(databaseNameFactory: sp => mongoDatabaseName, name: "mongodb");
            }
            else
            {
                // Check connection only (lists databases)
                healthChecksBuilder.AddMongoDb(name: "mongodb");
            }

            // Configure HealthChecks UI with in-memory storage
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(15);
                setup.MaximumHistoryEntriesPerEndpoint(60);
                setup.AddHealthCheckEndpoint(serviceName, "/health");
            })
            .AddInMemoryStorage();

            return services;
        }

        /// <summary>
        /// Configures FastTechFoods HealthChecks with both Entity Framework DbContext and MongoDB support.
        /// Note: You need to install the AspNetCore.HealthChecks.MongoDb package first.
        /// Example: Install-Package AspNetCore.HealthChecks.MongoDb
        /// </summary>
        /// <typeparam name="TDbContext">The Entity Framework DbContext type for database health checks</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configuration">The configuration object to read from</param>
        /// <param name="mongoConnectionString">MongoDB connection string</param>
        /// <param name="mongoDatabaseName">MongoDB database name (optional)</param>
        /// <returns>The IServiceCollection for chaining</returns>
        /// <remarks>
        /// This method requires the AspNetCore.HealthChecks.MongoDb package to be installed in your project.
        /// The actual MongoDB health check registration will only work if the package is available.
        /// </remarks>
        public static IServiceCollection AddFastTechFoodsHealthChecksWithDbContextAndMongoDB<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            string mongoConnectionString,
            string? mongoDatabaseName = null)
            where TDbContext : DbContext
        {
            // Get observability configuration section
            var observabilityConfig = configuration.GetSection("Observability");
            var serviceName = observabilityConfig["ServiceName"] ?? "FastTechFoods.Service";

            // Configure HealthChecks with DbContext
            var healthChecksBuilder = services.AddHealthChecks()
                .AddDbContextCheck<TDbContext>("database-context");
            
            // Register MongoClient as singleton (MongoDB recommendation)
            services.AddSingleton(sp => new MongoDB.Driver.MongoClient(mongoConnectionString));
            
            if (!string.IsNullOrEmpty(mongoDatabaseName))
            {
                // Check specific database
                healthChecksBuilder.AddMongoDb(databaseNameFactory: sp => mongoDatabaseName, name: "mongodb");
            }
            else
            {
                // Check connection only (lists databases)
                healthChecksBuilder.AddMongoDb(name: "mongodb");
            }

            // Configure HealthChecks UI with in-memory storage
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(15);
                setup.MaximumHistoryEntriesPerEndpoint(60);
                setup.AddHealthCheckEndpoint(serviceName, "/health");
            })
            .AddInMemoryStorage();

            return services;
        }
    }
}
