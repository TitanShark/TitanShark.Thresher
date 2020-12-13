using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public abstract class InterceptorsRunner : IDisposable
    {
        public IInterceptor[] Interceptors { get; }

        public bool HasAnyInterceptor => Interceptors != null && Interceptors.Length > 0;

        public HttpContentReader ContentReader { get; }

        protected InterceptorsRunner(params IInterceptor[] interceptors)
        {
            Interceptors = interceptors;

            if (Interceptors != null && Interceptors.Length > 0)
            {
                ContentReader = CreateContentReader(Interceptors.Length);

                foreach(var interceptor in Interceptors)
                {
                    interceptor.ContentReader = new WeakReference<HttpContentReader>(ContentReader);
                }
            }
        }

        public abstract Task OnPreparing(CallId callId, HttpRequestMessage request, CancellationToken cancellationToken);

        public abstract Task OnDone(CallId callId, HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken);

        public abstract Task OnError(CallId callId, HttpRequestMessage request, Exception exception, CancellationToken cancellationToken);

        public void Dispose()
        {
            if (ContentReader != null)
            {
                ContentReader.Dispose();
            }
        }

        protected virtual HttpContentReader CreateContentReader(int numberOfInterceptors)
        {
            return new HttpContentReader(numberOfInterceptors);
        }
    }
}
