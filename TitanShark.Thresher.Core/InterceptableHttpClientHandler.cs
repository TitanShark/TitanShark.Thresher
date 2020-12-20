using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class InterceptableHttpClientHandler : FlowableHttpClientHandler
    {
        public InterceptorsRunner InterceptorsRunner { get; }

        public InterceptableHttpClientHandler(Transmitter transmitter = null, InterceptorsRunner interceptorsRunner = null) 
            : base(transmitter)
        {
            InterceptorsRunner = interceptorsRunner;
        }

        protected override async Task OnPreparing(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (InterceptorsRunner != null)
            {
                await InterceptorsRunner.OnPreparing(callId, request, cancellationToken);
            }
        }

        protected override async Task OnDone(CallId callId, HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (InterceptorsRunner != null)
            {
                await InterceptorsRunner.OnDone(callId, request, response, cancellationToken);
            }
        }

        protected override async Task OnError(CallId callId, HttpRequestMessage request, Exception exception, CancellationToken cancellationToken = default)
        {
            if (InterceptorsRunner != null)
            {
                await InterceptorsRunner.OnError(callId, request, exception, cancellationToken);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (InterceptorsRunner != null)
            {
                InterceptorsRunner.Dispose();
            }
        }
    }
}

