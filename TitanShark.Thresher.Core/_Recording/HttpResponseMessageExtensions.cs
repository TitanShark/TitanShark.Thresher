using System.Net.Http;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<RecordableResponse> ToRecordable(this HttpResponseMessage response, CallId callId, HttpContentReader reader)
        {
            var recordable = new RecordableResponse
            {
                Headers = response.Headers,
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase,
                RequestMessage = response.RequestMessage,
                StatusCode = response.StatusCode,
                Version = response.Version
            };

            var content = response.Content;
            recordable.Content = await reader.ReadDirectly(callId, content);

            return recordable;
        }
    }
}
