using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<RecordableRequest> ToRecordable(this HttpRequestMessage request)
        {
            var recordable = new RecordableRequest 
            {
                Headers = request.Headers,
                Method = request.Method,
                Properties = request.Properties,
                RequestUri = request.RequestUri,
                Version = request.Version
            };

            if (request.Content != null)
            {
                // serializes the HTTP content to a memory buffer
                await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);

                using (var stream = new MemoryStream())
                {
                    await request.Content.CopyToAsync(stream).ConfigureAwait(false);
                    stream.Position = 0;

                    recordable.Content = stream.ToArray();
                }
            }

            return recordable;
        }
    }
}
