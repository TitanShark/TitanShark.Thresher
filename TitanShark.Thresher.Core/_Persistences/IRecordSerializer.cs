using System.Threading.Tasks;

namespace TitanShark.Thresher.Core
{
    public interface IRecordSerializer<T>
    {
        Task<T> Serialize(Record record);

        Task<Record> Deserialize(T json);
    }
}
