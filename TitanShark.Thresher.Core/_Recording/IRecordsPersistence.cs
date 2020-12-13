using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public interface IRecordsPersistence
    {
        Task Save(Record record, CancellationToken cancellationToken);

        Task<ISnapshot> Snapshot(CancellationToken cancellationToken, DateTime? from = null, DateTime? to = null, HttpStatusCode[] statusCodes = null);
    }
}
