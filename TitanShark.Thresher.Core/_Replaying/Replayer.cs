using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class Replayer
    {
        private CancellationTokenSource _cts;

        public ReplayingStrategy ReplayingStrategy { get; }

        public HttpClient Client { get; }

        public ISnapshot Snapshot { get; }

        public Replayer(ReplayingStrategy replayingStrategy, HttpClient client, ISnapshot snapshot)
        {
            ReplayingStrategy = replayingStrategy;
            Client = client;
            Snapshot = snapshot;
        }

        public Task Start()
        {
            if (_cts != null)
            {
                throw new InvalidOperationException("Please call Stop() first!");
            }

            if (Snapshot.TotalRecords == default)
            {
                return Task.CompletedTask;
            }

            _cts = new CancellationTokenSource();

            return ReplayingStrategy.Replay(Client, Snapshot, _cts.Token);
        }

        public virtual void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}