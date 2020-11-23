using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TitanShark.Thresher.Core
{
    public class RecordableRequest
    {
        public byte[] Content { get; set; }

        public HttpRequestHeaders Headers { get; set; }
        
        public HttpMethod Method { get; set; }

#if NET5_0
        public HttpRequestOptions Options { get; set;  }

        public HttpVersionPolicy VersionPolicy { get; set; }
#else
        public IDictionary<string, object> Properties { get; set; }
#endif

        public Uri RequestUri { get; set; }
        
        public Version Version { get; set; }
    }
}
