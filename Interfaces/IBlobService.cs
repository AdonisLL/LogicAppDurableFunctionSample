using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;

namespace TransferToBlobFunction.Interfaces
{
    public interface IBlobService
    {
        Task<BlobClient> GetBlobClient(string fileName);

        Task<bool> UploadBlobFromStream(string fileName, byte[] bytesContent);

        Task<bool> UploadBlobFromStream(string fileName, Stream stream);
    }
}
