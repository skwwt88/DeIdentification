using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeIdentification
{
    public class StorageTransformer : ITransformer
    {
        private string connectionString;

        public StorageTransformer(IConfiguration config)
        {
            this.connectionString = config["ResourceStorageConnectionString"];
        }

        public async Task<TransformResponse> TransformAsync(TransformRequest request, ILogger log)
        {
            var response = new TransformResponse()
            {
                OutputContainer = request.OutputContainer
            };

            var blobServiceClient = new BlobServiceClient(connectionString);

            BlobContainerClient inputContainerClient = blobServiceClient.GetBlobContainerClient(request.InputContainer);
            if (!await inputContainerClient.ExistsAsync())
            {
                log.LogError($"input container: {request.InputContainer} not exist.");
                throw new ArgumentException($"Container: {request.InputContainer} not exist");
            }

            BlobClient inputBlobClient = inputContainerClient.GetBlobClient(request.InputFileName);
            if (!await inputBlobClient.ExistsAsync())
            {
                log.LogError($"input blob: {request.InputFileName} not exist.");
                throw new ArgumentException($"Container: {request.InputFileName} not exist");
            }

            BlobContainerClient outputContainerClient = blobServiceClient.GetBlobContainerClient(request.OutputContainer);
            await outputContainerClient.CreateIfNotExistsAsync();

            BlobClient outputBlobClient = outputContainerClient.GetBlobClient(request.InputFileName);
            if (await outputBlobClient.DeleteIfExistsAsync())
            {
                log.LogInformation($"Delete existed {outputBlobClient.Name}.");
            }

            var downloadContent = await inputBlobClient.DownloadAsync();

            log.LogInformation($"Start to upload to blob: {outputBlobClient.Name}");
            await outputBlobClient.UploadAsync(downloadContent.Value.Content);
            log.LogInformation($"Upload to blob completed: {outputBlobClient.Name}");

            return response;
        }
    }
}
