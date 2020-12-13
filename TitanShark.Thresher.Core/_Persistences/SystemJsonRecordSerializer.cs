using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public class SystemJsonRecordSerializer : IRecordSerializer<string>
    {
        public Task<string> Serialize(Record record, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(record);

            return Task.FromResult(json);
        }

        public Task<Record> Deserialize(string json, CancellationToken cancellationToken)
        {
            var record = JsonSerializer.Deserialize<Record>(json);

            return Task.FromResult(record);
        }
    }
}
