using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace TitanShark.Thresher.Core
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<RecordableRequest> ToRecordable(this HttpRequestMessage request, CallId callId, HttpContentReader reader)
        {
            var recordable = new RecordableRequest 
            {
                Headers = request.Headers.ToDictionary(header => header.Key, header => header.Value),
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

            // content might be null
            recordable.Content = await reader.ReadDirectly(callId, content);
            recordable.ContentTypeName = content?.GetType()?.AssemblyQualifiedName;

            return recordable;
        }
    }
}
