using System;
using System.Threading.Tasks;
using Orleans;

namespace GrainOptionsMonitor
{
    internal interface IOptionsMonitorGrainInternal<TData> : IGrainWithIntegerKey
    {
        Task<OptionsMonitorGrainState<TData>> GetState(Guid? token);
    }
}
