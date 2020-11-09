using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public interface IRecordsPersistence
    {
        Task Save(Record record, CancellationToken cancellationToken);
    }
}
