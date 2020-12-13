using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class Transmitter
    {
        public Func<CallId, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> SendFunction { get; }

        public Transmitter(Func<CallId, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendFunction)
        {
            SendFunction = sendFunction ?? throw new ArgumentNullException(nameof(sendFunction));
        }
    }
}
