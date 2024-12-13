using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Scalar.AspNetCore;
using TrailMail.ServiceDefaults;
using TrailMail.WebApi.Context;
using TrailMail.WebApi.Endpoints;
using TrailMail.WebApi.Endpoints.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

#region EF

builder.AddNpgsqlDbContext<AppDbContext>("TrailMail",
    configureDbContextOptions: options => { options.UseOpenIddict(); }
);

#endregion

#region Semantic Kernel

builder.Services.AddSingleton<IChatCompletionService>((sp) =>
{
#pragma warning disable SKEXP0010
    return new OpenAIChatCompletionService(
        builder.Configuration["Groq:ModelId"] ?? "llama-3.1-70b-versatile",
        endpoint: new Uri("https://api.groq.com/openai/v1"),
        apiKey: builder.Configuration["Groq:ApiKey"] ?? throw new Exception("Groq API Key is missing.")
    );
#pragma warning restore SKEXP0010
});

builder.Services.AddTransient<Kernel>((sp) =>
{
    var kernel = new Kernel(sp);
    return kernel;
});

#endregion

#region Auth

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options
            .UseEntityFrameworkCore()
            .UseDbContext<AppDbContext>();
    })
    .AddClient((options) =>
    {
        options.AllowAuthorizationCodeFlow();

        options
            .AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options
            .UseAspNetCore()
            .EnableRedirectionEndpointPassthrough();

        options
            .UseWebProviders()
            .AddLinkedIn((linkedInOptions) =>
            {
                linkedInOptions
                    .SetClientId(builder.Configuration["LinkedIn:ClientId"] ?? "86ttxw6vxo9d9h")
                    .SetClientSecret(
                        builder.Configuration["LinkedIn:ClientSecret"] ??
                        throw new Exception("LinkedIn Client Secret is missing.")
                    )
                    .SetRedirectUri("/Auth/Callback/LinkedIn");
            });
    });

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.LoginPath = "/Auth/Login"; });

builder.Services.AddAuthorization();

#endregion

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();

app.Run();