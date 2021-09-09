using Orleans;

namespace GrainOptionsMonitor.Extensions
{
    public static class GrainFactoryExtensions
    {
        public static T GetGrain<T>(this IGrainFactory factory)
            where T : class, IOptionsMonitorGrain =>
            factory?.GetGrain<T>(0);
    }
}
