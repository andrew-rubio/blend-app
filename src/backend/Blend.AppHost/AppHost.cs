var builder = DistributedApplication.CreateBuilder(args);

// Register Cosmos DB (emulator in development, Azure in production)
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsEmulator();

// Register the API project and wire it to Cosmos DB
var api = builder.AddProject<Projects.Blend_Api>("blend-api")
    .WithReference(cosmos)
    .WaitFor(cosmos);

builder.Build().Run();
