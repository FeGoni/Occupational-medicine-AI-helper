using Microsoft.Extensions.AI;
using OllamaSharp.Models.Chat;
using AiChatMessage = Microsoft.Extensions.AI.ChatMessage;
using AiChatRole = Microsoft.Extensions.AI.ChatRole;

namespace ZbadAI;

public record ChatTurn(string Role, string Content);
public record ChatRequestDto(List<ChatTurn>? Messages);

public static class ChatEndpoint
{
    private const int MaxMessages = 20;
    private const int MaxMessageLength = 4000;

    public static async Task Handle(ChatRequestDto request, IChatClient chatClient, ModelSettings model,
        SystemPrompt systemPrompt, HttpContext ctx, ILogger<ChatRequestDto> logger)
    {
        var messages = request.Messages;
        string? error = Validate(messages);
        if (error is not null)
        {
            await WriteError(ctx, StatusCodes.Status400BadRequest, error);
            return;
        }

        // Instrukcje systemowe dokłada wyłącznie serwer; klient może wysłać tylko wiadomości user/assistant.
        var history = new List<AiChatMessage> { new(AiChatRole.System, systemPrompt.Text) };
        history.AddRange(messages!
            .Select(m => new AiChatMessage(m.Role == "user" ? AiChatRole.User : AiChatRole.Assistant, m.Content)));

        var options = new ChatOptions { Temperature = model.Temperature };
        if (string.IsNullOrWhiteSpace(model.Endpoint))
            options.RawRepresentationFactory = _ => new ChatRequest { Think = model.Think };

        bool started = false;
        try
        {
            await foreach (var update in chatClient.GetStreamingResponseAsync(history, options, ctx.RequestAborted))
            {
                // update.Text zawiera tylko treść odpowiedzi (bez rozumowania modelu).
                if (string.IsNullOrEmpty(update.Text)) continue;

                if (!started)
                {
                    ctx.Response.ContentType = "text/plain; charset=utf-8";
                    started = true;
                }
                await ctx.Response.WriteAsync(update.Text, ctx.RequestAborted);
                await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
            }
        }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        {
            // Użytkownik przerwał żądanie.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Błąd wywołania modelu {Model}", model.Name);
            const string msg = "Model jest niedostępny. Sprawdź, czy Ollama działa i czy model jest pobrany.";
            if (!started)
                await WriteError(ctx, StatusCodes.Status503ServiceUnavailable, msg);
            else
                await ctx.Response.WriteAsync($"\n\n[{msg}]");
        }
    }

    private static string? Validate(List<ChatTurn>? messages)
    {
        if (messages is null || messages.Count == 0) return "Brak wiadomości.";
        if (messages.Count > MaxMessages) return $"Rozmowa jest za długa (maks. {MaxMessages} wiadomości). Zacznij nową rozmowę.";
        if (messages.Any(m => m.Role is not ("user" or "assistant"))) return "Niedozwolona rola wiadomości.";
        if (messages.Any(m => string.IsNullOrWhiteSpace(m.Content))) return "Pusta wiadomość.";
        if (messages.Any(m => m.Content.Length > MaxMessageLength)) return $"Wiadomość jest za długa (maks. {MaxMessageLength} znaków).";
        if (messages[^1].Role != "user") return "Ostatnia wiadomość musi pochodzić od użytkownika.";
        return null;
    }

    private static async Task WriteError(HttpContext ctx, int status, string message)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "text/plain; charset=utf-8";
        await ctx.Response.WriteAsync(message);
    }
}
