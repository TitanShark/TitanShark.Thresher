using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class SerialInterceptionHttpClientHandler : InterceptableHttpClientHandler
    {
        public SerialInterceptionHttpClientHandler(Transmitter transmitter = null, params IInterceptor[] interceptors) : base(transmitter, interceptors)
        {
        }

        protected async Task InvokeInterceptorsSerially(Func<IInterceptor, CancellationToken, Task> func, CancellationToken cancellationToken)
        {
            if (!HasAnyInterceptor || func == null)
            {
                return;
            }

            var enabledInterceptors = Interceptors.Where(hook => hook.IsEnabled).ToArray();

            foreach (var interceptor in enabledInterceptors)
            {
                await func(interceptor, cancellationToken);
            }
        }

        protected override async Task OnPreparing(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await InvokeInterceptorsSerially(async (interceptor, ct) => await interceptor.OnPreparing(callId, request, ct), cancellationToken);
        }

        protected override async Task OnDone(CallId callId, HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            await InvokeInterceptorsSerially(async (interceptor, ct) => await interceptor.OnDone(callId, request, response, ct), cancellationToken);
        }

        protected override async Task OnError(CallId callId, HttpRequestMessage request, Exception exception, CancellationToken cancellationToken)
        {
            await InvokeInterceptorsSerially(async (interceptor, ct) => await interceptor.OnError(callId, request, exception, ct), cancellationToken);
        }
    }
}
