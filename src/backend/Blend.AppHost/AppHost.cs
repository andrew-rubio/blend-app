var builder = DistributedApplication.CreateBuilder(args);

// Register the API project
var api = builder.AddProject<Projects.Blend_Api>("blend-api");

builder.Build().Run();
