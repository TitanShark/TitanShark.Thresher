using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public interface ISnapshot
    {
        int TotalRecords { get; }

        Task<Record[]> Peek(int position, int batchSize, CancellationToken cancellationToken);
    }
}