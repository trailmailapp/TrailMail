using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.TextGeneration;
using Scalar.AspNetCore;
using TrailMail.WebApi.Options;

var builder = WebApplication.CreateBuilder(args);

#region Semantic Kernel

builder.Services.AddOptions<HuggingFaceOptions>()
    .Bind(builder.Configuration.GetSection("HuggingFace"));

builder.Services.AddKeyedSingleton<ITextGenerationService>("HuggingFace", (sp, _) =>
{
#pragma warning disable SKEXP0070

    return new HuggingFaceTextGenerationService(
        "microsoft/Phi-3-mini-4k-instruct",
        apiKey: sp.GetRequiredService<IOptions<HuggingFaceOptions>>().Value.ApiKey
    );

#pragma warning restore SKEXP0070
});

builder.Services.AddTransient<Kernel>((sp) =>
{
    var pluginCollection = new KernelPluginCollection();
    return new Kernel(sp, pluginCollection);
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

app.MapGet("/TextGeneration", async ([FromKeyedServices("HuggingFace")] ITextGenerationService textGenerationService) =>
    {
        var i = await textGenerationService.GetTextContentsAsync(
            """
            Write a greeting.
            """
        );
        
        return i;
    })
    .WithName("TextGeneration");

app.Run();