using System;
using System.Reflection;
using GrainOptionsMonitor.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using TestSilo.Grains;

namespace TestSilo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseOrleans((context, builder) =>
                {
                    builder.UseLocalhostClustering()
                        .AddMemoryGrainStorageAsDefault()
                        .UseDashboard(options =>
                        {
                            options.CounterUpdateIntervalMs = 5000;
                            options.HideTrace = true;
                            options.HostSelf = false;
                        });

                    builder.Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "test-silo";
                            options.ServiceId = $"silo-{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}";
                        })
                        .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)
                        .ConfigureApplicationParts(x => x.AddApplicationPart(typeof(ConfigurationGrain).Assembly)
                            .WithReferences());

                    builder.AddGrainOptionMonitor<ConfigurationGrainState>();
                });
    }
}
