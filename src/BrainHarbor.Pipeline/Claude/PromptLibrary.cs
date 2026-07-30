using Microsoft.Extensions.Options;

namespace BrainHarbor.Pipeline.Claude;

/// <summary>
/// Loads versioned prompt templates from the Prompts directory (shipped beside
/// the app) and caches them. Templates are versioned artifacts — one place to
/// find them, parsed once.
/// </summary>
public sealed class PromptLibrary(IOptions<ClaudeOptions> options)
{
    private readonly Dictionary<string, PromptTemplate> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();

    private string Directory => options.Value.PromptsDirectory
        ?? Path.Combine(AppContext.BaseDirectory, "Prompts");

    /// <summary>Loads Prompts/&lt;name&gt;.md.</summary>
    public PromptTemplate Get(string name)
    {
        lock (_gate)
        {
            if (_cache.TryGetValue(name, out var cached))
            {
                return cached;
            }

            var path = Path.Combine(Directory, $"{name}.md");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Prompt template '{name}' not found at {path}.", path);
            }

            var template = PromptTemplate.Load(path);
            _cache[name] = template;
            return template;
        }
    }
}
