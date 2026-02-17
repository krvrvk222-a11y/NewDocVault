using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read configuration
var cosmosEndpoint = builder.Configuration["Cosmos:Endpoint"];
var cosmosKey = builder.Configuration["Cosmos:Key"];
var databaseName = builder.Configuration["Cosmos:DatabaseName"];
var containerName = builder.Configuration["Cosmos:ContainerName"];

var storageConnectionString = builder.Configuration["Storage:ConnectionString"];
var blobContainerName = builder.Configuration["Storage:ContainerName"];

// Register CosmosClient
builder.Services.AddSingleton(s =>
{
    return new CosmosClient(cosmosEndpoint, cosmosKey);
});

// Register BlobContainerClient
builder.Services.AddSingleton(s =>
{
    var blobServiceClient = new BlobServiceClient(storageConnectionString);
    return blobServiceClient.GetBlobContainerClient(blobContainerName);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
