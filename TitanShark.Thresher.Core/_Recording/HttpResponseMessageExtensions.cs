using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace TitanShark.Thresher.Core
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<RecordableResponse> ToRecordable(this HttpResponseMessage response, CallId callId, HttpContentReader reader)
        {
            var recordable = new RecordableResponse
            {
                Headers = response.Headers.ToDictionary(header => header.Key, header => header.Value),
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase,
                RequestMessage = response.RequestMessage,
                StatusCode = response.StatusCode,
                Version = response.Version,
#if NET5_0
                TrailingHeaders = response.TrailingHeaders.ToDictionary(header => header.Key, header => header.Value)
#endif
            };

            var content = response.Content;

            // content can be null
            recordable.Content = await reader.ReadDirectly(callId, content);
            recordable.ContentTypeName = content?.GetType()?.AssemblyQualifiedName;

            return recordable;
        }
    }
}
