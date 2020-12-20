using Realms;

namespace TitanShark.Thresher.Realm
{
    public class RealmRequest : EmbeddedObject
    {
        [MapTo("method")]
        public string Method { get; set; }

        [MapTo("requestUri")]
        public string RequestUri { get; set; }
    }
}
