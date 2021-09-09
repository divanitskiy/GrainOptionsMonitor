using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans;

namespace GrainOptionsMonitor
{
    internal class GrainOptionsMonitor<TData> : IOptionsMonitor<TData>
    {
        private readonly IGrainFactory _grainFactory;
        private readonly Timer _timer;
        private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(15);
        private readonly SemaphoreSlim _semaphoreSlim = new(1);

        private event Action<TData, string> _onChange;
        private TData _value;
        private IOptionsMonitorGrainInternal<TData> _repositoryGrain;
        private Guid? _repositoryToken;


        public GrainOptionsMonitor(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
            _timer = new Timer(TimerCallback, null, TimeSpan.Zero, _timerInterval);
        }

        private async void TimerCallback(object state)
        {
            var hasLock = false;

            try
            {
                hasLock = await _semaphoreSlim.WaitAsync(0);

                if (!hasLock)
                {
                    return;
                }

                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                await PullRepository(null);
            }
            finally
            {
                if (hasLock)
                {
                    _semaphoreSlim.Release();
                    _timer.Change(_timerInterval, _timerInterval);
                }
            }
        }

        public TData CurrentValue =>
            GetValue();

        public TData Get(string name) =>
            GetValue();

        public IDisposable OnChange(Action<TData, string> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            _onChange += disposable.OnChange;

            return disposable;
        }

        private TData GetValue() => _value;

        private async Task PullRepository(object _)
        {
            _repositoryGrain ??= _grainFactory.GetGrain<IOptionsMonitorGrainInternal<TData>>(0);
            var repositoryState = await _repositoryGrain.GetState(_repositoryToken);
            if (repositoryState != null)
            {
                _value = repositoryState.Data;
                _onChange?.Invoke(repositoryState.Data, Options.DefaultName);
                _repositoryToken = repositoryState.Token;
            }
        }

        internal class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<TData, string> _listener;
            private readonly GrainOptionsMonitor<TData> _monitor;

            public ChangeTrackerDisposable(GrainOptionsMonitor<TData> monitor, Action<TData, string> listener)
            {
                _listener = listener;
                _monitor = monitor;
            }

            public void OnChange(TData options, string name) => _listener.Invoke(options, name);

            public void Dispose() => _monitor._onChange -= OnChange;
        }
    }
}
