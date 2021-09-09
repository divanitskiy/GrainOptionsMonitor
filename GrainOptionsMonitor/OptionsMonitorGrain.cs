using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace GrainOptionsMonitor
{
    public class OptionsMonitorGrain<TData> : Grain, IOptionsMonitorGrain<TData>, IOptionsMonitorGrainInternal<TData>
    {
        protected readonly IPersistentState<OptionsMonitorGrainState<TData>> Storage;

        protected OptionsMonitorGrain(IPersistentState<OptionsMonitorGrainState<TData>> state) =>
            Storage = state ?? throw new ArgumentNullException(nameof(state));

        public Task<TData> GetValue() =>
            Task.FromResult(Storage.State.Data);

        public virtual async Task SetValue(TData value)
        {
            Storage.State.Data = value;
            await SaveChangesAsync();
        }

        public Task<OptionsMonitorGrainState<TData>> GetState(Guid? token)
        {
            if (!token.HasValue || Storage.State.Token != token)
            {
                return Task.FromResult(Storage.State);
            }

            return Task.FromResult((OptionsMonitorGrainState<TData>)null);
        }

        protected async Task SaveChangesAsync()
        {
            Storage.State.Token = Guid.NewGuid();
            await Storage.WriteStateAsync();
        }
    }
}
