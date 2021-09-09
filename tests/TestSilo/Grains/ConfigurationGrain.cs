using GrainOptionsMonitor;
using Orleans.Runtime;

namespace TestSilo.Grains
{
    public class ConfigurationGrain : OptionsMonitorGrain<ConfigurationGrainState>, IConfigurationGrain
    {
        public ConfigurationGrain(
            [PersistentState("state")]IPersistentState<OptionsMonitorGrainState<ConfigurationGrainState>> state) 
            : base(state)
        {
        }
    }
}
