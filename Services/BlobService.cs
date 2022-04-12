using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using TransferToBlobFunction.Interfaces;
using TransferToBlobFunction.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TransferToBlobFunction.Services
{
    public class BlobService : IBlobService
    {
        private BlobConfiguration _blobConfiguration;
        public BlobService(IOptions<BlobConfiguration> blobConfiguration)
        {
            _blobConfiguration = blobConfiguration.Value;
        }

        public async Task<BlobClient> GetBlobClient(string fileName)
        {
            // Create a BlobServiceClient object which will be used to create a container client

            var credential = new DefaultAzureCredential();

            var token = credential.GetToken(
                new Azure.Core.TokenRequestContext(
                    new[] { "https://graph.microsoft.com/.default" }));

            var accessToken = token.Token;

            BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(_blobConfiguration.StorageUri), credential);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_blobConfiguration.ContainerName);
            if (containerClient == null)
            {
                containerClient = await blobServiceClient.CreateBlobContainerAsync(_blobConfiguration.ContainerName);
            }

            // Get a reference to a blob
            return containerClient.GetBlobClient(fileName);
        }

        public async Task<bool> UploadBlobFromStream(string fileName, Stream stream)
        {
            BlobClient blobClient = await GetBlobClient(fileName);
            var result = await blobClient.UploadAsync(stream, true);

            return true;
        }

        public async Task<bool> UploadBlobFromStream(string fileName, byte[] bytesContent)
        {
            BlobClient blobClient = await GetBlobClient(fileName);

            using (var stream = new MemoryStream(bytesContent, writable: false))
            {
                var result = await blobClient.UploadAsync(stream);
            }

            return true;
        }
    }
}