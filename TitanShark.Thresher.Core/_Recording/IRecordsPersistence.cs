using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public interface IRecordsPersistence
    {
        Task Save(Record record, CancellationToken cancellationToken = default);

        Task<ISnapshot> Snapshot(DateTime? from = null, DateTime? to = null, HttpStatusCode[] statusCodes = null, CancellationToken cancellationToken = default);
    }
}
