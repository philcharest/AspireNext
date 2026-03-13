var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
// 1. Reference your .NET Server
var server = builder.AddProject<Projects.AspireNext_Server>("server")
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();
// 2. Reference the Next.js Frontend using the .esproj
// Note: "frontend" here matches the name in the Projects namespace
builder.AddJavaScriptApp("frontend", "../frontend")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(server);

builder.Build().Run();
