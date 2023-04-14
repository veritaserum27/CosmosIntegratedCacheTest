using CosmosIntegratedCacheTest.Repositories;
using CosmosIntegratedCacheTest.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

[assembly: FunctionsStartup(typeof(CosmosIntegratedCacheTest.Startup))]
namespace CosmosIntegratedCacheTest
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ICosmosDbRepository, CosmosDbRepository>();
            builder.Services.AddSingleton<ITestItemService, TestItemService>();
            builder.Services.AddSingleton<IOpenApiConfigurationOptions>(_ => new OpenApiConfigurationOptions()
            {
                Info = new OpenApiInfo()
                {
                    Version = "1.0.0",
                    Title = "Cosmos Integrated Cache Test",
                    Description = "This service explores caching behavior via queries that use the dedicated gateway and queries that don't."
                },
                Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                OpenApiVersion = OpenApiVersionType.V2,
                IncludeRequestingHostName = true,
                ForceHttps = false,
                ForceHttp = false,
            });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(builder.GetContext().ApplicationRootPath)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            builder.ConfigurationBuilder.AddConfiguration(configurationRoot);
        }
    }
}