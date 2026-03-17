using Azure.Provisioning.AppContainers;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var acaEnv = builder.AddAzureContainerAppEnvironment("aca-env")
                    .WithAzdResourceNaming(); // Keeps naming consistent with azd
// 1. Reference your .NET Server
var server = builder.AddProject<Projects.AspireNext_Server>("server")
    .WithReference(cache)
    .WaitFor(cache)
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
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(server)
    .PublishAsAzureContainerApp((infrastructure, app) =>
    {
        app.Template.Scale.MinReplicas = 0; // Scales down to $0 when idle
    });

builder.Build().Run();
