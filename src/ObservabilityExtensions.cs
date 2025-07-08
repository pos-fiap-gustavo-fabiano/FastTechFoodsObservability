using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using HealthChecks.UI.Client;
using Prometheus;
using Npgsql;

namespace FastTechFoods.Observability
{
    public static class ObservabilityExtensions
    {
        /// <summary>
        /// Configures FastTechFoods observability with hardcoded parameters for backward compatibility.
        /// For new implementations, consider using AddFastTechFoodsObservabilityAndHealthChecks with IConfiguration.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="serviceName">The name of the service</param>
        /// <param name="serviceVersion">The version of the service</param>
        /// <param name="otlpEndpoint">The OTLP endpoint URL</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFastTechFoodsObservability(
            this IServiceCollection services,
            string serviceName,
            string serviceVersion,
            string otlpEndpoint)
        {
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion))
                .WithTracing(tracing => tracing
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddNpgsql()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    }))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    }));
            
            services.AddLogging(logging =>
            {
                logging.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;

                    options.AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri(otlpEndpoint);
                        exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                });
            });
            
            return services;
        }

        /// <summary>
        /// Configures comprehensive FastTechFoods observability (OpenTelemetry + Serilog) and HealthChecks 
        /// using configuration values from the 'Observability' section.
        /// </summary>
        /// <typeparam name="TDbContext">The Entity Framework DbContext type for database health checks</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configuration">The configuration object to read from</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFastTechFoodsObservabilityAndHealthChecks<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TDbContext : DbContext
        {
            // Get observability configuration section
            var observabilityConfig = configuration.GetSection("Observability");
            var serviceName = observabilityConfig["ServiceName"] ?? "FastTechFoods.Service";
            var serviceVersion = observabilityConfig["ServiceVersion"] ?? "1.0.0";
            var otlpEndpoint = observabilityConfig["OtlpEndpoint"] ?? "http://localhost:4317";
            
            // Configure Serilog
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("ServiceVersion", serviceVersion);

            if (!string.IsNullOrEmpty(otlpEndpoint))
            {
                loggerConfiguration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.ResourceAttributes.Add("service.name", serviceName);
                    options.ResourceAttributes.Add("service.version", serviceVersion);
                });
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            services.AddSerilog();

            // Configure OpenTelemetry
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion))
                .WithTracing(tracing => tracing
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddNpgsql()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    }))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    }));

            // Configure logging with OpenTelemetry
            services.AddLogging(logging =>
            {
                logging.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;

                    options.AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri(otlpEndpoint);
                        exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                });
            });

            // Configure HealthChecks
            services.AddHealthChecks()
                .AddDbContextCheck<TDbContext>();

            // Configure HealthChecks UI with in-memory storage
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(15);
                setup.MaximumHistoryEntriesPerEndpoint(60);
                setup.AddHealthCheckEndpoint("default api", "/health");
            })
            .AddInMemoryStorage();

            return services;
        }

        /// <summary>
        /// Configures comprehensive FastTechFoods observability and HealthChecks with custom database health check.
        /// </summary>
        /// <typeparam name="TDbContext">The Entity Framework DbContext type for database health checks</typeparam>
        /// <typeparam name="THealthCheck">The custom database health check implementation</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configuration">The configuration object to read from</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFastTechFoodsObservabilityAndHealthChecks<TDbContext, THealthCheck>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TDbContext : DbContext
            where THealthCheck : class, Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
        {
            // Configure observability (same as above)
            services.AddFastTechFoodsObservabilityAndHealthChecks<TDbContext>(configuration);

            // Remove the default DbContext health check and add custom one
            services.AddHealthChecks()
                .AddCheck<THealthCheck>("database");

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
            // Configure Prometheus metrics endpoint
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

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
        /// Configures Prometheus metrics for FastTechFoods applications.
        /// This method can be used to add Prometheus metrics to existing applications.
        /// </summary>
        /// <param name="app">The IApplicationBuilder to configure</param>
        /// <returns>The IApplicationBuilder for chaining</returns>
        public static IApplicationBuilder UseFastTechFoodsPrometheus(this IApplicationBuilder app)
        {
            // Configure Prometheus metrics endpoint
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            
            return app;
        }

        /// <summary>
        /// Configures Prometheus metrics collection and export for FastTechFoods applications.
        /// This method adds Prometheus metrics to the OpenTelemetry pipeline.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="serviceName">The name of the service for metrics labeling</param>
        /// <param name="serviceVersion">The version of the service for metrics labeling</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFastTechFoodsPrometheus(
            this IServiceCollection services,
            string serviceName,
            string serviceVersion)
        {
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter());

            return services;
        }

        /// <summary>
        /// Configures Prometheus metrics collection and export using configuration values.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configuration">The configuration object to read from</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFastTechFoodsPrometheus(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var observabilityConfig = configuration.GetSection("Observability");
            var serviceName = observabilityConfig["ServiceName"] ?? "FastTechFoods.Service";
            var serviceVersion = observabilityConfig["ServiceVersion"] ?? "1.0.0";

            return services.AddFastTechFoodsPrometheus(serviceName, serviceVersion);
        }
    }
}
