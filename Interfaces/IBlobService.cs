using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointFileToBlob.Interfaces
{
    public interface IBlobService
    {
        Task<BlobClient> GetBlobClient(string fileName);

        Task<bool> UploadBlobFromStream(string fileName, byte[] bytesContent);
    }
}
