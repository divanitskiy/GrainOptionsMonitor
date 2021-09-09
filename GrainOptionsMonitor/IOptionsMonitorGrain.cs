using System.Threading.Tasks;
using Orleans;

namespace GrainOptionsMonitor
{
    public interface IOptionsMonitorGrain<TData> : IOptionsMonitorGrain
    {
        Task<TData> GetValue();
        Task SetValue(TData value);
    }

    public interface IOptionsMonitorGrain : IGrainWithIntegerKey
    {
    }
}
