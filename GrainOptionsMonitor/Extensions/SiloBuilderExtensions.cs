using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;

namespace GrainOptionsMonitor.Extensions
{
    public static class SiloBuilderExtensions
    {
        /// <summary>
        /// Setups all monitors based on all GrainRepository implementations found in referenced assemblies.
        /// </summary>
        /// <param name="builder">The silo builder.</param>
        public static void AddGrainOptionMonitors(this ISiloBuilder builder)
        {
            var repositoryGrainTypes = Assembly.GetCallingAssembly()
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => x.IsGrainRepositoryType())
                .ToList();

            foreach (var grainType in repositoryGrainTypes)
            {
                var dataType = grainType.GetGenericRepositoryTypeArgument();
                typeof(SiloBuilderExtensions)
                    .GetMethod(nameof(AddGrainOptionMonitor))
                    ?.MakeGenericMethod(dataType)
                    .Invoke(null, new[] { builder });
            }
        }

        /// <summary>
        /// Setups a single option monitor
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="builder">The silo builder.</param>
        public static void AddGrainOptionMonitor<TData>(this ISiloBuilder builder)
        {
            builder.ConfigureServices(x =>
            {
                x.AddSingleton<GrainOptionsMonitor<TData>>();
                x.AddSingleton<IOptionsMonitor<TData>>(provider => provider.GetRequiredService<GrainOptionsMonitor<TData>>());
            });

            builder.AddStartupTask((services, _) =>
            {
                // init a monitor
                services.GetRequiredService<IOptionsMonitor<TData>>();
                return Task.CompletedTask;
            });
        }

        private static bool IsGrainRepositoryType(this Type type)
        {
            if (!type.IsClass || !typeof(IOptionsMonitorGrain).IsAssignableFrom(type))
            {
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(OptionsMonitorGrain<>))
            {
                return false;
            }

            return GetGenericRepositoryTypeArgument(type) != null;
        }

        private static Type GetGenericRepositoryTypeArgument(this Type type)
        {
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(OptionsMonitorGrain<>))
                {
                    return type.GetGenericArguments().First();
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
