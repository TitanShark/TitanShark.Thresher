using System;

namespace TitanShark.Thresher.Core
{
    public class Record
    {
        public CallId CallId { get; set; }

        public RecordableRequest Request { get; set; }

        public RecordableResponse Response { get; set; }

        public Exception Error { get; set; }
    }
}
