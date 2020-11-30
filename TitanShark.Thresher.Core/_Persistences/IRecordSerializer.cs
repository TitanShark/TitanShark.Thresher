using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public interface IRecordSerializer<T>
    {
        Task<T> Serialize(Record record, CancellationToken cancellationToken);

        Task<Record> Deserialize(T json, CancellationToken cancellationToken);
    }
}
