using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public abstract class ReplayingStrategy
    {
        public int CurrentPosition { get; protected set; }

        public int BatchSize { get; set; }

        public Func<Type, byte[], HttpContent> ContentResolver { get; }

        protected ReplayingStrategy(Func<Type, byte[], HttpContent> contentResolver = null)
        {
            ContentResolver = contentResolver;
        }

        public async Task Replay(HttpClient client, ISnapshot snapshot, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var records = await snapshot.Peek(CurrentPosition, BatchSize, cancellationToken);
                var totalInBatch = records.Length;

                if (totalInBatch > 0)
                {
                    CurrentPosition += totalInBatch;

                    await OnReplay(client, records, cancellationToken);
                }
                else
                {
                    break;
                }
            }
        }

        protected abstract Task OnReplay(HttpClient client, Record[] records, CancellationToken cancellationToken = default);
    }
}