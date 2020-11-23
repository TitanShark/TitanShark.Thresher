using System.Net.Http;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<RecordableRequest> ToRecordable(this HttpRequestMessage request, CallId callId, HttpContentReader reader)
        {
            var recordable = new RecordableRequest 
            {
                Headers = request.Headers,
                Method = request.Method,
#if NET5_0
                Options = request.Options,
                VersionPolicy = request.VersionPolicy,
#else
                Properties = request.Properties,
#endif
                RequestUri = request.RequestUri,
                Version = request.Version
            };

            var content = request.Content;
            recordable.Content = await reader.ReadDirectly(callId, content);

            return recordable;
        }
    }
}
