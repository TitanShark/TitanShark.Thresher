using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TitanShark.Thresher.Core
{
    public class RecordableResponse
    {
        public byte[] Content { get; set; }

        public HttpResponseHeaders Headers { get; set;  }
        
        public bool IsSuccessStatusCode { get; set; }
        
        public string ReasonPhrase { get; set; }
        
        public HttpRequestMessage RequestMessage { get; set; }
        
        public HttpStatusCode StatusCode { get; set; }
        
        public Version Version { get; set; }

#if NET5_0
        public HttpHeaders TrailingHeaders { get; set; }
#endif
    }
}
