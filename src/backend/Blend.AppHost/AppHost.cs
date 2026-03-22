var builder = DistributedApplication.CreateBuilder(args);

// Register Cosmos DB (emulator in development, Azure in production)
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsEmulator();

// Register Azure Blob Storage (Azurite emulator in development, Azure in production)
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();
var blobs = storage.AddBlobs("blobs");

// Register the API project and wire it to Cosmos DB and Blob Storage
var api = builder.AddProject<Projects.Blend_Api>("blend-api")
    .WithReference(cosmos)
    .WithReference(blobs)
    .WaitFor(cosmos);

builder.Build().Run();
