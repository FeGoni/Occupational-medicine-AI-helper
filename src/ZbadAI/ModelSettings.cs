namespace ZbadAI;

public class ModelSettings
{
    /// <summary>Nazwa modelu, np. qwen3:8b (Ollama) albo nazwa modelu na PLGrid.</summary>
    public string Name { get; set; } = "qwen3:8b";

    /// <summary>Adres lokalnej Ollamy (używany, gdy Endpoint jest pusty).</summary>
    public string OllamaUrl { get; set; } = "http://localhost:11434";

    /// <summary>Endpoint API zgodnego z OpenAI. Pusty = lokalna Ollama.</summary>
    public string? Endpoint { get; set; }

    public float Temperature { get; set; } = 0.2f;

    /// <summary>Tryb myślenia (qwen3). Tylko dla Ollamy; wyłączony skraca czas odpowiedzi.</summary>
    public bool Think { get; set; } = false;
}
