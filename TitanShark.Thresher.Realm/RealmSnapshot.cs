using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TitanShark.Thresher.Core;

namespace TitanShark.Thresher.Realm
{
    public class RealmSnapshot : ISnapshot
    {
        protected readonly IRecordSerializer<string> Serializer;
        protected readonly RealmConfiguration RealmConfiguration;
        protected readonly DateTime? From;
        protected readonly DateTime? To;
        protected readonly HttpStatusCode[] StatusCodes;

        public RealmSnapshot(IRecordSerializer<string> serializer, RealmConfiguration realmConfiguration, DateTime? from = null, DateTime? to = null, HttpStatusCode[] statusCodes = null)
        {
            Serializer = serializer;
            RealmConfiguration = realmConfiguration;
            From = from;
            To = to;
            StatusCodes = statusCodes;
        }

        public int TotalRecords 
        { 
            get 
            {
                using (var instance = Realms.Realm.GetInstance(RealmConfiguration))
                {
                    return Query(instance).Count();
                }
            } 
        }

        public virtual async Task<Record[]> Peek(int position, int batchSize, CancellationToken cancellationToken = default)
        {
            using (var instance = Realms.Realm.GetInstance(RealmConfiguration))
            {
                try
                {
                    // see also: https://realm.io/docs/dotnet/latest/api/linqsupport.html#partitioning-operators
                    var realmRecords = Query(instance)
                                        .ToList() // finish the real query
                                        .Skip(position)
                                        .Take(batchSize);

                    var tasks = new List<Task<Record>>();
                    foreach (var realmRecord in realmRecords)
                    {
                        // at this moment, Realm Record will be downloaded!
                        var jsonRecord = realmRecord.JsonRecord;

                        tasks.Add(Serializer.Deserialize(jsonRecord, cancellationToken));
                    }

                    var records = await Task.WhenAll(tasks);
                    return records;
                }
                catch (Exception exception)
                {
#if DEBUG
                    System.Diagnostics.Debug.Fail(exception.ToString());
#endif
                    throw;
                }
            }
        }

        protected virtual IQueryable<RealmRecord> Query(Realms.Realm instance)
        {
            var records = instance.All<RealmRecord>();

            if (From.HasValue)
            {
                records = records.Where(record => record.UtcCreated >= From.Value);
            }

            if (To.HasValue)
            {
                records = records.Where(record => record.UtcCreated <= To.Value);
            }

            string filter = string.Empty;
            if (StatusCodes != null && StatusCodes.Any())
            {
                filter = string.Join(" OR ", StatusCodes.Select(sc => $"response.statusCode = {(int)sc}"));
            }
            if (!string.IsNullOrEmpty(filter))
            {
                records = records.Filter(filter);
            }

            return records;
        }
    }
}
