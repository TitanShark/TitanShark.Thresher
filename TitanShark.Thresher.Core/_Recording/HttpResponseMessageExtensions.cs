using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<RecordableResponse> ToRecordable(this HttpResponseMessage response)
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

            if (response.Content != null)
            {
                // serializes the HTTP content to a memory buffer
                await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);

                using (var stream = new MemoryStream())
                {
                    await response.Content.CopyToAsync(stream).ConfigureAwait(false);
                    stream.Position = 0;

                    recordable.Content = stream.ToArray();
                }
            }

            return recordable;
        }
    }
}
