using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Sas;
using Azure.Storage.Blobs;
using DocVault.Api.Models;

namespace DocVault.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly Container _container;
        private readonly BlobContainerClient _blobContainer;

        public DocumentsController(
            CosmosClient cosmosClient,
            BlobServiceClient blobServiceClient,
            IConfiguration configuration)
        {
            var databaseName = configuration["Cosmos:DatabaseName"];
            var containerName = configuration["Cosmos:ContainerName"];
            var blobContainerName = configuration["Storage:ContainerName"];

            _container = cosmosClient.GetContainer(databaseName, containerName);
            _blobContainer = blobServiceClient.GetBlobContainerClient(blobContainerName);

            _blobContainer.CreateIfNotExists();
        }

        // =========================
        // UPLOAD DOCUMENT
        // =========================
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var id = Guid.NewGuid().ToString();
            var userId = "test-user"; // Later replace with Entra ID user

            var blobClient = _blobContainer.GetBlobClient(id);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            var document = new DocumentItem
            {
                Id = id,
                UserId = userId,
                FileName = file.FileName,
                BlobUrl = blobClient.Uri.ToString(),
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                UploadedAt = DateTime.UtcNow.ToString("o"),
                Status = "pending"
            };

            await _container.CreateItemAsync(document, new PartitionKey(userId));

            return Ok(document);
        }

        // =========================
        // GET ALL DOCUMENTS (With SAS)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = "test-user";

            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.userId = @userId ORDER BY c.uploadedAt DESC")
                .WithParameter("@userId", userId);

            var iterator = _container.GetItemQueryIterator<DocumentItem>(queryDefinition);

            var results = new List<object>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();

                foreach (var doc in response)
                {
                    var blobClient = _blobContainer.GetBlobClient(doc.Id);

                    var sasBuilder = new BlobSasBuilder
                    {
                        BlobContainerName = _blobContainer.Name,
                        BlobName = doc.Id,
                        Resource = "b",
                        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
                    };

                    sasBuilder.SetPermissions(BlobSasPermissions.Read);

                    var sasUrl = blobClient.GenerateSasUri(sasBuilder);

                    results.Add(new
                    {
                        doc.Id,
                        doc.FileName,
                        doc.SizeBytes,
                        doc.UploadedAt,
                        DownloadUrl = sasUrl.ToString()
                    });
                }
            }

            return Ok(results);
        }
    }
}
