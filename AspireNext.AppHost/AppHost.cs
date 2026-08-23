using Azure.Provisioning.AppContainers;
using Azure.Provisioning.PostgreSql;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgresUsername = builder.AddParameter("postgres-username", "postgres");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

// Provisions a real Azure Database for PostgreSQL Flexible Server (Burstable) when published,
// while local `dotnet run` still gets a plain Postgres container - same as before this change.
// Password auth (rather than the Flexible Server default of Entra-only) keeps the existing
// Npgsql/AddNpgsqlDbContext client code on the server unchanged in both environments.
var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .WithPasswordAuthentication(postgresUsername, postgresPassword)
    .ConfigureInfrastructure(infra =>
    {
        var flexibleServer = infra.GetProvisionableResources().OfType<PostgreSqlFlexibleServer>().Single();
        flexibleServer.Sku = new PostgreSqlFlexibleServerSku
        {
            Name = "Standard_B1ms",
            Tier = PostgreSqlFlexibleServerSkuTier.Burstable,
        };
    })
    .RunAsContainer(container =>
    {
        container.WithHostPort(5432);
        container.WithDataVolume();
    });
var catalogDb = postgres.AddDatabase("catalogdb");

var stripeSecretKey = builder.AddParameter("stripe-secret-key", secret: true);
var stripeWebhookSecret = builder.AddParameter("stripe-webhook-secret", secret: true);

var smtpHost = builder.AddParameter("smtp-host");
var smtpPort = builder.AddParameter("smtp-port");
var smtpUsername = builder.AddParameter("smtp-username", secret: true);
var smtpPassword = builder.AddParameter("smtp-password", secret: true);
var smtpFromAddress = builder.AddParameter("smtp-from-address");
var smtpFromName = builder.AddParameter("smtp-from-name");

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
    .WithEnvironment("Smtp__Host", smtpHost)
    .WithEnvironment("Smtp__Port", smtpPort)
    .WithEnvironment("Smtp__Username", smtpUsername)
    .WithEnvironment("Smtp__Password", smtpPassword)
    .WithEnvironment("Smtp__FromAddress", smtpFromAddress)
    .WithEnvironment("Smtp__FromName", smtpFromName)
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
