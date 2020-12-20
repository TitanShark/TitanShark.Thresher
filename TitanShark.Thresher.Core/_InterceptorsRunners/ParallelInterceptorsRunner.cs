using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class ParallelInterceptorsRunner : InterceptorsRunner
    {
        public ParallelInterceptorsRunner(params IInterceptor[] interceptors)
            : base(interceptors)
        {
        }

        protected virtual async Task RunInterceptorsInParallel(Func<IInterceptor, CancellationToken, Task> func, CancellationToken cancellationToken = default)
        {
            if (!HasAnyInterceptor || func == null)
            {
                return;
            }

            var enabledInterceptors = Interceptors.Where(interceptor => interceptor.IsEnabled).ToArray();

            var tasks = enabledInterceptors.Select(interceptor => func(interceptor, cancellationToken));

            await Task.WhenAll(tasks);
        }

        public override async Task OnPreparing(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            await RunInterceptorsInParallel(async (interceptor, ct) => await interceptor.OnPreparing(callId, request, ct), cancellationToken);
        }

        public override async Task OnDone(CallId callId, HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            await RunInterceptorsInParallel(async (interceptor, ct) => await interceptor.OnDone(callId, request, response, ct), cancellationToken);
        }

        public override async Task OnError(CallId callId, HttpRequestMessage request, Exception exception, CancellationToken cancellationToken = default)
        {
            await RunInterceptorsInParallel(async (interceptor, ct) => await interceptor.OnError(callId, request, exception, ct), cancellationToken);
        }
    }
}
