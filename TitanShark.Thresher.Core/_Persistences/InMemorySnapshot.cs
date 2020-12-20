using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class InMemorySnapshot : ISnapshot
    {
        public int TotalRecords => SortedRecords == null ? default : SortedRecords.Count;

        protected ConcurrentBag<Record> UnsortedRecords { get; private set; } = new ConcurrentBag<Record>();

        protected ICollection<Record> SortedRecords { get; private set; }

        internal void AddRecord(Record record)
        {
            UnsortedRecords.Add(record);
        }

        internal void SortRecordsUponCreationUtc()
        {
            SortedRecords = UnsortedRecords.OrderBy(record => record.CallId.UtcCreated).ToArray();
        }

        public Task<Record[]> Peek(int position, int batchSize, CancellationToken cancellationToken = default)
        {
            var result = SortedRecords.Skip(position).Take(batchSize).ToArray();
            return Task.FromResult(result);
        }
    }
}
