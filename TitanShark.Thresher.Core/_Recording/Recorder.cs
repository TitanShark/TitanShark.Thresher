using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class Recorder : IInterceptor
    {
        public WeakReference<HttpContentReader> ContentReader { get; set; }

        public bool IsEnabled { get; set; } = true;

        public IRecordsPersistence[] Persistences { get; }

        public Recorder(params IRecordsPersistence[] persistences)
        {
            Persistences = persistences;
        }

        public virtual Task OnPreparing(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            // does nothing
            return Task.CompletedTask;
        }

        public virtual async Task OnDone(CallId callId, HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (Persistences == null || Persistences.Length == 0)
            {
                return;
            }

            var record = new Record
            {
                CallId = callId,
                Request = await request.ToRecordable(callId, GetContentReader()),
                Response = await response.ToRecordable(callId, GetContentReader())
            };

            await SaveToPersistences(record, cancellationToken);
        }

        public virtual async Task OnError(CallId callId, HttpRequestMessage request, Exception exception, CancellationToken cancellationToken = default)
        {
            if (Persistences == null || Persistences.Length == 0)
            {
                return;
            }

            var record = new Record
            {
                CallId = callId,
                Request = await request.ToRecordable(callId, GetContentReader()),
                Error = exception
            };

            await SaveToPersistences(record, cancellationToken);
        }

        protected virtual async Task SaveToPersistences(Record record, CancellationToken cancellationToken = default)
        {
            var tasks = Persistences.Select(persistence => persistence.Save(record, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private HttpContentReader GetContentReader()
        {
            ContentReader.TryGetTarget(out HttpContentReader target);
            return target;
        }
    }
}
