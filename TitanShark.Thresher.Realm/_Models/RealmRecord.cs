using MongoDB.Bson;
using Realms;
using System;

namespace TitanShark.Thresher.Realm
{
    public class RealmRecord : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [MapTo("callId")]
        public string CallId { get; set; }

        [MapTo("utcCreated")]
        public DateTimeOffset UtcCreated { get; set; }

        [MapTo("request")]
        public RealmRequest Request { get; set; }

        [MapTo("response")]
        public RealmResponse Response { get; set; }

        [MapTo("jsonRecord")]
        public string JsonRecord { get; set; }
    }
}
