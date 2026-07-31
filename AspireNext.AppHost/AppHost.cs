using Azure.Provisioning.AppContainers;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var catalogDb = postgres.AddDatabase("catalogdb");

var acaEnv = builder.AddAzureContainerAppEnvironment("aca-env")
                    .WithAzdResourceNaming(); // Keeps naming consistent with azd
// 1. Reference your .NET Server
var server = builder.AddProject<Projects.AspireNext_Server>("server")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .PublishAsAzureContainerApp((infrastructure, app) =>
    {
        app.Template.Scale.MinReplicas = 0; // Scales down to $0 when idle
        app.Configuration.ActiveRevisionsMode = ContainerAppActiveRevisionsMode.Single; // Link to the environment resource
    });

// 2. Reference the Next.js Frontend using the .esproj
// Note: "frontend" here matches the name in the Projects namespace
builder.AddJavaScriptApp("frontend", "../frontend")
    .WithHttpEndpoint(port: 80, targetPort: 3000, name: "http") // Maps external 80 to internal 3000
    .WithExternalHttpEndpoints()
    .WithReference(server)
    // Add this to ensure azd performs a deployment
    .PublishAsDockerFile()
    .PublishAsAzureContainerApp((infrastructure, app) =>
    {
        app.Template.Scale.MinReplicas = 0; // Scales down to $0 when idle
    });

builder.Build().Run();
