using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<TrailMail_WebApi>("api");
var app = builder.AddNpmApp("app", "../TrailMail.WebApp", "dev")
            .WithReference(api);

builder.Build().Run();