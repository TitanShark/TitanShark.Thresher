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
                Properties = request.Properties,
                RequestUri = request.RequestUri,
                Version = request.Version
            };

            var content = request.Content;
            recordable.Content = await reader.ReadDirectly(callId, content);

            return recordable;
        }
    }
}
