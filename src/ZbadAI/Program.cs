using System.ClientModel;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using ZbadAI;

var builder = WebApplication.CreateBuilder(args);

var model = builder.Configuration.GetSection("Model").Get<ModelSettings>() ?? new ModelSettings();
builder.Services.AddSingleton(model);
builder.Services.AddSingleton(CreateChatClient(model));
builder.Services.AddSingleton(SystemPrompt.Load(AppContext.BaseDirectory));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/chat", ChatEndpoint.Handle);

app.Run();

// Bez endpointu: lokalna Ollama; z endpointem: API zgodne z OpenAI (np. PLGrid https://llm.hpc.psnc.pl/v1)
static IChatClient CreateChatClient(ModelSettings model)
{
    if (string.IsNullOrWhiteSpace(model.Endpoint))
        return new OllamaApiClient(new Uri(model.OllamaUrl), model.Name);

    var apiKey = Environment.GetEnvironmentVariable("LLM_API_KEY")
        ?? throw new InvalidOperationException("Brak zmiennej środowiskowej LLM_API_KEY (wymagana dla Model:Endpoint).");

    return new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri(model.Endpoint) })
        .GetChatClient(model.Name)
        .AsIChatClient();
}
