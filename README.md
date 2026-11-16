# ZbadAI

**Autorzy:**
- Hubert Zalewski
- Paweł Zakrzewski


Agent AI dla pacjentów przychodni medycyny pracy: na podstawie czynników ze skierowania podpowiada, jakie badania obejmuje wizyta i jak się do niej przygotować. Pełna wizja: [docs/wizja.md](docs/wizja.md).
Plan architektury: [docs/architecture.md](docs/Architecture.md).

## Uruchomienie

Wymagania: .NET 10 SDK oraz [Ollama](https://ollama.com/download).

```powershell
ollama pull qwen3:8b
dotnet run --project src/ZbadAI
```

Potem otwórz adres podany w konsoli (domyślnie http://localhost:5266).

Model ustawia się w `src/ZbadAI/appsettings.json` (sekcja `Model`). Żeby użyć API zgodnego z OpenAI (np. PLGrid), wpisz `Endpoint` (np. `https://llm.hpc.psnc.pl/v1`) i nazwę modelu, a klucz ustaw w zmiennej środowiskowej:

```powershell
$env:LLM_API_KEY = "..."
```

Instrukcje agenta: `src/ZbadAI/Prompts/system-prompt.md`. Kontekst (wyciąg z rozporządzenia i informacje przychodni): `src/ZbadAI/Knowledge/`. Po zmianie tych plików zrestartuj aplikację.
