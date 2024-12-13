using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("Postgres")
    .WithPgAdmin();

var db = postgres.AddDatabase("TrailMail");

var api = builder.AddProject<TrailMail_WebApi>("Api")
    .WithReference(db);

var app = builder.AddNpmApp("App", "../TrailMail.WebApp", "dev")
    .WithReference(api);

builder.Build().Run();