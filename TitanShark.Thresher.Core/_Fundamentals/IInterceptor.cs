using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public interface IInterceptor
    {
        WeakReference<HttpContentReader> ContentReader { get; set; }

        bool IsEnabled { get; set; }

        Task OnPreparing(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken);
        
        Task OnDone(CallId callId, HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken);

        Task OnError(CallId callId, HttpRequestMessage request, Exception exception, CancellationToken cancellationToken);
    }
}
