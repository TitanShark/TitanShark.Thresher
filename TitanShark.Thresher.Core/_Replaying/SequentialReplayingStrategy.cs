using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class SequentialReplayingStrategy : ReplayingStrategy
    {
        public SequentialReplayingStrategy(Func<Type, byte[], HttpContent> contentResolver = null)
            : base(contentResolver)
        {
            CurrentPosition = 0;
            BatchSize = 1;
        }

        protected override async Task OnReplay(HttpClient client, Record[] records, CancellationToken cancellationToken = default)
        {
            if (records == null || records.Length == 0)
            {
                return;
            }

            foreach(var record in records)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var message = record.Request.GetRequestMessage(ContentResolver);

                await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}