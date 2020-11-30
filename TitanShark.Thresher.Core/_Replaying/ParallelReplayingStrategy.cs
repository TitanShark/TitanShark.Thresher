using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class ParallelReplayingStrategy : ReplayingStrategy
    {
        public ParallelReplayingStrategy(Func<Type, byte[], HttpContent> contentResolver = null)
            : base(contentResolver)
        {
            CurrentPosition = 0;
            BatchSize = 1;
        }

        protected override async Task OnReplay(HttpClient client, Record[] records, CancellationToken cancellationToken)
        {
            if (records == null || records.Length == 0)
            {
                return;
            }

            var tasks = new List<Task>();

            foreach (var record in records)
            {
                tasks.Add(Task.Run(() => 
                {
                    var message = record.Request.GetRequestMessage(ContentResolver);

                    return client.SendAsync(message, cancellationToken).ConfigureAwait(false);
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
    }
}