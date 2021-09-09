using System.Threading.Tasks;
using Orleans;

namespace TestSilo.Grains
{
    public interface IConfigurationGrain : IGrainWithIntegerKey
    {
        Task<ConfigurationGrainState> GetValue();
        Task SetValue(ConfigurationGrainState value);
    }
}
