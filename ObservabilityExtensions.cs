using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FastTechFoods.Observability
{
    public static class ObservabilityExtensions
    {
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
                    .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
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
    }
}
