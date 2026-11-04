namespace ZbadAI;

/// <summary>
/// Instrukcje agenta (Prompts/system-prompt.md) + kontekst (wszystkie pliki z Knowledge/) pod nagłówkiem # KONTEKST.
/// Składane raz przy starcie aplikacji; zmiana plików wymaga restartu.
/// </summary>
public record SystemPrompt(string Text)
{
    public static SystemPrompt Load(string baseDir)
    {
        var instructions = File.ReadAllText(Path.Combine(baseDir, "Prompts", "system-prompt.md"));

        var knowledge = Directory.GetFiles(Path.Combine(baseDir, "Knowledge"), "*.md")
            .Order()
            .Select(File.ReadAllText);

        return new SystemPrompt($"{instructions.Trim()}\n\n# KONTEKST\n\n{string.Join("\n\n", knowledge)}");
    }
}
