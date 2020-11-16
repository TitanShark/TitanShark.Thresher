using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public abstract class InterceptableHttpClientHandler : HttpClientHandler
    {
        internal IList<IInterceptor> Interceptors { get; } = new List<IInterceptor>();

        internal Transmitter Transmitter { get; }
        
        public bool HasAnyInterceptor => Interceptors != null && Interceptors.Count > 0;

        protected InterceptableHttpClientHandler(Transmitter transmitter = null, params IInterceptor[] interceptors)
        {
            Transmitter = transmitter ?? CreateDefaultTransmitter();

            if (interceptors != null && interceptors.Length > 0)
            {
                foreach(var interceptor in interceptors) 
                {
                    Interceptors.Add(interceptor);
                }
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var callId = CallId.CreateFromGuid();

            await OnPreparing(callId, request, cancellationToken);

            try
            {
                var response = await OnSending(callId, request, cancellationToken);

                await OnDone(callId, request, response, cancellationToken);

                return response;
            }
            catch(Exception exception)
            {
                await OnError(callId, request, exception, cancellationToken);

                throw;
            }
        }

        protected virtual Transmitter CreateDefaultTransmitter()
        {
            return new Transmitter
            (
                (callId, request, cancellationToken) => base.SendAsync(request, cancellationToken)
            );
        }

        protected virtual Task<HttpResponseMessage> OnSending(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sendFunction = Transmitter?.SendFunction;

            if (sendFunction != null)
            {
                return sendFunction(callId, request, cancellationToken);
            }

            return Task.FromResult(default(HttpResponseMessage));
        }

        protected abstract Task OnPreparing(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken);

        protected abstract Task OnDone(CallId callId, HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken);

        protected abstract Task OnError(CallId callId, HttpRequestMessage request, Exception exception, CancellationToken cancellationToken);
    }
}
