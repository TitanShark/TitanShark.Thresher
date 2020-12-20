using Realms;

namespace TitanShark.Thresher.Realm
{
    public class RealmResponse : EmbeddedObject
    {
        [MapTo("isSuccessStatusCode")]
        public bool IsSuccessStatusCode { get; set; }

        [MapTo("statusCode")]
        public int StatusCode { get; set; }
    }
}
