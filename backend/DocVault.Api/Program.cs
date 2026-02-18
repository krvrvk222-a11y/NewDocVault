using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========================
// COSMOS CONFIG
// ========================

var cosmosEndpoint = builder.Configuration["Cosmos:Endpoint"];
var cosmosKey = builder.Configuration["Cosmos:Key"];

if (string.IsNullOrEmpty(cosmosEndpoint))
    throw new Exception("Cosmos Endpoint is NULL");

if (string.IsNullOrEmpty(cosmosKey))
    throw new Exception("Cosmos Key is NULL");

builder.Services.AddSingleton(s =>
{
    return new CosmosClient(cosmosEndpoint, cosmosKey);
});

// ========================
// BLOB CONFIG
// ========================

var storageConnectionString =
    builder.Configuration["Storage:ConnectionString"];

if (string.IsNullOrEmpty(storageConnectionString))
    throw new Exception("Storage ConnectionString is NULL");

builder.Services.AddSingleton(s =>
{
    return new BlobServiceClient(storageConnectionString);
});

// ========================
// CORS
// ========================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
