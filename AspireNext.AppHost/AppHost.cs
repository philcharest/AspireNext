using Azure.Provisioning.AppContainers;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres", port: 5432)
    .WithDataVolume();
var catalogDb = postgres.AddDatabase("catalogdb");

var stripeSecretKey = builder.AddParameter("stripe-secret-key", secret: true);
var stripeWebhookSecret = builder.AddParameter("stripe-webhook-secret", secret: true);

var acaEnv = builder.AddAzureContainerAppEnvironment("aca-env")
                    .WithAzdResourceNaming(); // Keeps naming consistent with azd

// 1. Reference the Next.js Frontend using the .esproj
// Note: "frontend" here matches the name in the Projects namespace
// Port 80 only makes sense once deployed behind Azure Container Apps ingress - locally the
// Next.js dev server actually listens on 3000, so pinning 80 here too made service discovery
// (services__frontend__http__0, used to build the Stripe success/cancel URLs) resolve to the
// wrong port for local runs.
var isPublishingFrontend = builder.ExecutionContext.IsPublishMode;
var frontend = builder.AddJavaScriptApp("frontend", "../frontend")
    .WithHttpEndpoint(
        port: isPublishingFrontend ? 80 : 3000,
        targetPort: 3000,
        name: "http",
        isProxied: isPublishingFrontend) // proxying requires port != targetPort for non-container resources
    .WithExternalHttpEndpoints();

// 2. Reference your .NET Server
var server = builder.AddProject<Projects.AspireNext_Server>("server")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithReference(frontend) // so the server can build Stripe success/cancel URLs pointing back at the frontend
    .WithEnvironment("Stripe__SecretKey", stripeSecretKey)
    .WithEnvironment("Stripe__WebhookSecret", stripeWebhookSecret)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .PublishAsAzureContainerApp((infrastructure, app) =>
    {
        app.Template.Scale.MinReplicas = 0; // Scales down to $0 when idle
        app.Configuration.ActiveRevisionsMode = ContainerAppActiveRevisionsMode.Single; // Link to the environment resource
    });

frontend.WithReference(server)
    // Add this to ensure azd performs a deployment
    .PublishAsDockerFile()
    .PublishAsAzureContainerApp((infrastructure, app) =>
    {
        app.Template.Scale.MinReplicas = 0; // Scales down to $0 when idle
    });

builder.Build().Run();
