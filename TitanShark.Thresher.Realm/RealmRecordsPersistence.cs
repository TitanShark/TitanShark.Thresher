using Realms;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TitanShark.Thresher.Core;

namespace TitanShark.Thresher.Realm
{
    public class RealmRecordsPersistence : IRecordsPersistence
    {
        protected const string DefaultDbFilePath = "./persistence.realm";

        protected IRecordSerializer<string> Serializer { get; }

        protected string DbFilePath { get; }

        public RealmRecordsPersistence(string dbFilePath = DefaultDbFilePath) 
            : this(new SystemJsonRecordSerializer(), dbFilePath)
        { 
        }

        public RealmRecordsPersistence(IRecordSerializer<string> serializer, string dbFilePath = DefaultDbFilePath)
        {
            Serializer = serializer;
            DbFilePath = dbFilePath;
        }

        protected virtual RealmConfiguration CreateRealmConfiguration(string dbFilePath)
        {
            return new RealmConfiguration(dbFilePath);
        }

        public async Task Save(Record record, CancellationToken cancellationToken = default)
        {
            var data = await Serializer.Serialize(record, cancellationToken);

            using (var instance = Realms.Realm.GetInstance(CreateRealmConfiguration(DbFilePath)))
            {
                instance.Write(() =>
                {
                    var realmRecord = new RealmRecord
                    {
                        CallId = record.CallId.Id,
                        UtcCreated = record.CallId.UtcCreated,
                        Request = record.Request == null
                                    ? null
                                    : new RealmRequest
                                    {
                                        Method = record.Request.Method.ToString().ToLowerInvariant(),
                                        RequestUri = record.Request.RequestUri.ToString()
                                    },
                        Response = record.Response == null
                                    ? null
                                    : new RealmResponse
                                    {
                                        IsSuccessStatusCode = record.Response.IsSuccessStatusCode,
                                        StatusCode = (int) record.Response.StatusCode
                                    },
                        JsonRecord = data
                    };

                    instance.Add(realmRecord);
                });
            }
        }

        public virtual Task<ISnapshot> Snapshot(DateTime? from = null, DateTime? to = null, HttpStatusCode[] statusCodes = null, CancellationToken cancellationToken = default)
        {
            ISnapshot snapshot = new RealmSnapshot(Serializer, CreateRealmConfiguration(DbFilePath), from, to, statusCodes);
            return Task.FromResult(snapshot);
        }
    }
}
