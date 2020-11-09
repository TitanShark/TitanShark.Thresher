using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class InMemoryRecordsPersistence : IRecordsPersistence
    {
        public IReadOnlyDictionary<CallId, string> ReadOnlyRecords => Records;

        protected ConcurrentDictionary<CallId, string> Records { get; } = new ConcurrentDictionary<CallId, string>();
        
        protected IRecordSerializer<string> Serializer { get; }

        public InMemoryRecordsPersistence() : this(new SystemJsonRecordSerializer())
        {
        }

        public InMemoryRecordsPersistence(IRecordSerializer<string> serializer)
        {
            Serializer = serializer;
        }
        
        public async Task Save(Record record, CancellationToken cancellationToken)
        {
            Records[record.CallId] = await Serializer.Serialize(record);
        }
    }
}
