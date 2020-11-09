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
        
        public IDictionary<string, object> Properties { get; set;  }
        
        public Uri RequestUri { get; set; }
        
        public Version Version { get; set; }
    }
}
