using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class HttpContentReader : IDisposable
    {
        private readonly ConcurrentDictionary<CallId, SemaphoreSlim> _semaphores;
        private readonly int _numberOfInterceptors;

        public HttpContentReader(int numberOfInterceptors)
        {
            _semaphores = new ConcurrentDictionary<CallId, SemaphoreSlim>();
            _numberOfInterceptors = numberOfInterceptors;
        }

        ~HttpContentReader()
        {
            Dispose();
        }

        public async Task<byte[]> ReadWithLoadingIntoBuffer(CallId callId, HttpContent content)
        {
            if (content == null)
            {
                return new byte[0];
            }

            SemaphoreSlim semaphore = Enter(callId);
            await semaphore.WaitAsync();

            // serializes the HTTP content to a memory buffer
            await content.LoadIntoBufferAsync().ConfigureAwait(false);

            byte[] result = null;

            using (var stream = new MemoryStream())
            {
                await content.CopyToAsync(stream).ConfigureAwait(false);
                stream.Position = 0;

                result = stream.ToArray();
            }

            Release(callId, semaphore);

            return result;
        }

        public async Task<byte[]> ReadDirectly(CallId callId, HttpContent content)
        {
            if (content == null)
            {
                return new byte[0];
            }

            SemaphoreSlim semaphore = Enter(callId);
            await semaphore.WaitAsync();

            byte[] result = await content.ReadAsByteArrayAsync();

            Release(callId, semaphore);

            return result;
        }

        public void Dispose()
        {
            if (_semaphores.Any())
            {
                foreach(var semaphore in _semaphores)
                {
                    semaphore.Value.Dispose();
                }

                _semaphores.Clear();
            }
        }

        private SemaphoreSlim Enter(CallId callId)
        {
            if (!_semaphores.TryGetValue(callId, out SemaphoreSlim semaphore))
            {
                semaphore = new SemaphoreSlim(1, _numberOfInterceptors);
                _semaphores[callId] = semaphore;
            }

            return semaphore;
        }

        private void Release(CallId callId, SemaphoreSlim semaphore)
        {
            if (semaphore.Release() == _numberOfInterceptors)
            {
                _semaphores.TryRemove(callId, out semaphore);
                semaphore.Dispose();
            }
        }
    }
}
