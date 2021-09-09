using System;

namespace GrainOptionsMonitor
{
    public class OptionsMonitorGrainState<TData>
    {
        public Guid Token { get; set; }
        public TData Data { get; set; }
    }
}
