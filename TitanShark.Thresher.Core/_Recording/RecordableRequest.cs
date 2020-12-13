using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TitanShark.Thresher.Core
{
    public class RecordableRequest
    {
        public byte[] Content { get; set; }

        public string ContentTypeName { get; set; }

        public Dictionary<string, IEnumerable<string>> Headers { get; set; }
        
        public HttpMethod Method { get; set; }

#if NET5_0
        public HttpRequestOptions Options { get; set;  }

        public HttpVersionPolicy VersionPolicy { get; set; }
#else
        public IDictionary<string, object> Properties { get; set; }
#endif

        public Uri RequestUri { get; set; }
        
        public Version Version { get; set; }

        public HttpRequestMessage GetRequestMessage(Func<Type, byte[], HttpContent> contentResolver = null)
        {
            var result = new HttpRequestMessage
            {
                Method = Method,
                RequestUri = RequestUri,
                Version = Version
            };

            var contentType = !string.IsNullOrEmpty(ContentTypeName) 
                                ? Type.GetType(ContentTypeName) 
                                : null;

            if (contentType != null && Content != null)
            {
                if (typeof(ByteArrayContent).IsAssignableFrom(contentType))
                {
                    result.Content = new ByteArrayContent(Content);
                }
                else
                {
                    if (contentResolver != null)
                    {
                        result.Content = contentResolver(contentType, Content);
                    }
                    else
                    {
                        throw new NotSupportedException($"Content-Type '{contentType}' is not supported.");
                    }
                }
            }

            if (Headers != null)
            {
                foreach (var header in Headers)
                {
                    result.Headers.Add(header.Key, header.Value);
                }
            }

#if NET5_0
            foreach (var option in Options)
            {
                result.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);
            }

            result.VersionPolicy = VersionPolicy;
#else
            foreach (var property in Properties)
            {
                result.Properties.Add(property.Key, property.Value);
            }
#endif

            return result;
        }
    }
}
