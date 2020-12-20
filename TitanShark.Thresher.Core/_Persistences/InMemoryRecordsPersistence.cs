using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public virtual async Task Save(Record record, CancellationToken cancellationToken = default)
        {
            Records[record.CallId] = await Serializer.Serialize(record, cancellationToken);
        }

        public virtual Task<ISnapshot> Snapshot(DateTime? from = null, DateTime? to = null, HttpStatusCode[] statusCodes = null, CancellationToken cancellationToken = default)
        {
            var matchedKvps = !from.HasValue && !to.HasValue 
                                    ? Records.ToArray() // see also: https://referencesource.microsoft.com/#mscorlib/system/Collections/Concurrent/ConcurrentDictionary.cs,edad672303ee9ee3
                                    : Records.Where(kvp => !from.HasValue || kvp.Key.UtcCreated >= from.Value)
                                             .Where(kvp => !to.HasValue || kvp.Key.UtcCreated <= to.Value)
                                             .ToArray();

            var jsonRecords = matchedKvps.Select(kvp => kvp.Value);

            var snapshot = new InMemorySnapshot();

            Parallel.ForEach(jsonRecords, async (jsonRecord) => 
            { 
                if (!cancellationToken.IsCancellationRequested)
                {
                    var record = await Serializer.Deserialize(jsonRecord, cancellationToken);

                    if (statusCodes == null || !statusCodes.Any() ||
                        (record.Response != null && statusCodes.Contains(record.Response.StatusCode)))
                    {
                        snapshot.AddRecord(record);
                    }
                }
            });

            snapshot.SortRecordsUponCreationUtc();

            return Task.FromResult<ISnapshot>(snapshot);
        }
    }
}
