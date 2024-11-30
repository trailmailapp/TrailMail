using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Scalar.AspNetCore;
using TrailMail.WebApi.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

#region Semantic Kernel

builder.Services.AddOptions<GroqOptions>()
    .Bind(builder.Configuration.GetSection("Groq"));

builder.Services.AddSingleton<IChatCompletionService>((sp) =>
{
#pragma warning disable SKEXP0010
    return new OpenAIChatCompletionService(
        sp.GetRequiredService<IOptions<GroqOptions>>().Value.ModelId,
        endpoint: new Uri("https://api.groq.com/openai/v1"),
        apiKey: sp.GetRequiredService<IOptions<GroqOptions>>().Value.ApiKey
    );
#pragma warning restore SKEXP0010
});

builder.Services.AddTransient<Kernel>((sp) =>
{
    var kernel = new Kernel(sp);
    return kernel;
});

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
}

app.UseHttpsRedirection();

app.Run();
