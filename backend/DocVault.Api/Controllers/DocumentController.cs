using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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
            BlobContainerClient blobContainer)
        {
            _container = cosmosClient
                .GetDatabase("docvaultdb")
                .GetContainer("documents");

            _blobContainer = blobContainer;
        }

        // =========================
        // POST: Upload Document
        // =========================
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var fileId = Guid.NewGuid().ToString();
            var blobClient = _blobContainer.GetBlobClient(fileId);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            var document = new DocumentItem
            {
                Id = fileId,
                UserId = "test-user",
                FileName = file.FileName,
                BlobUrl = blobClient.Uri.ToString(),
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                UploadedAt = DateTime.UtcNow.ToString("o"),
                Status = "pending"
            };

            await _container.CreateItemAsync(
                document,
                new PartitionKey("test-user"));

            return Ok(document);
        }

        // =========================
        // GET: List Documents
        // =========================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.userId = @userId")
                .WithParameter("@userId", "test-user");

            var iterator = _container.GetItemQueryIterator<dynamic>(query);

            var results = new List<DocumentItem>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();

                foreach (var item in response)
                {
                    var doc = new DocumentItem
                    {
                        Id = item.id,
                        UserId = item.userId,
                        FileName = item.fileName,
                        BlobUrl = item.blobUrl,
                        ContentType = item.contentType,
                        SizeBytes = item.sizeBytes,
                        UploadedAt = item.uploadedAt,
                        Status = item.status
                    };

                    results.Add(doc);
                }
            }

            return Ok(results);
        }
    }
}
