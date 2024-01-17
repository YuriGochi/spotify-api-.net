using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Filters;
using Serilog.Exceptions;
using Nest;
using Serilog.Sinks.Elasticsearch;

namespace spotify_consumer
{
        public static class LogExtension
        {
            public static void AddSerilogElastichApi(this IServiceCollection services, IConfiguration configuration)
            {

                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Elasticsearch(
                    options:
                        new ElasticsearchSinkOptions(
                            new Uri(configuration["Elasticsearch:Uri"]))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                            IndexFormat = configuration["Elasticsearch:Index"] + "-{0:yyyy.MM}"
                        })
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithCorrelationId()
                    .Enrich.WithProperty("ApplicationName", $"API Serilog - {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}")
                    .WriteTo.Async(wt => wt.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))
                    .CreateLogger();

                var url = configuration["Elasticsearch:Uri"];
                var defaultIndex = configuration["Elasticsearch:Index"];

                var settings = new ConnectionSettings(new Uri(url))
                    // .BasicAuthentication(userName, pass)
                    .PrettyJson()
                    .DefaultIndex(defaultIndex);

                //AddDefaultMappings(settings);

                var client = new ElasticClient(settings);

                services.AddSingleton<IElasticClient>(client);
            }
        }
}
