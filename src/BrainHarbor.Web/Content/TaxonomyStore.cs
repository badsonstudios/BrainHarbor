using YamlDotNet.Serialization;

namespace BrainHarbor.Web.Content;

/// <summary>One tumor type in the closed taxonomy (data-model.md).</summary>
public sealed record TumorType
{
    [YamlMember(Alias = "slug")]
    public string Slug { get; init; } = "";

    [YamlMember(Alias = "label")]
    public string Label { get; init; } = "";

    [YamlMember(Alias = "parent")]
    public string? Parent { get; init; }

    [YamlMember(Alias = "also")]
    public List<string> Also { get; init; } = [];
}

internal sealed class TaxonomyFile
{
    [YamlMember(Alias = "tumor_types")]
    public List<TumorType> TumorTypes { get; set; } = [];
}

/// <summary>Result of filtering classifier tags: what survived, what didn't.</summary>
public sealed record TagFilterResult(string[] Known, string[] Rejected);

/// <summary>
/// WI-201: loads Content/taxonomy.yml — the single source of truth for tumor
/// slugs. The classifier may only emit these (content-pipeline.md §9), so
/// <see cref="FilterTags"/> is the gate that stops an invented tumor type
/// reaching a patient.
///
/// The taxonomy is a tree: tag an item with the most specific type, and
/// <see cref="Matches"/> handles "does this item belong under filter X" by
/// walking ancestors. Callers must not flatten it themselves.
/// </summary>
public sealed class TaxonomyStore
{
    private readonly Dictionary<string, TumorType> _bySlug = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _aliasToSlug = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<TumorType> TumorTypes { get; }

    public TaxonomyStore(IWebHostEnvironment environment, IConfiguration configuration)
        : this(File.ReadAllText(
            configuration["Content:TaxonomyFile"]
            ?? Path.Combine(environment.ContentRootPath, "Content", "taxonomy.yml")))
    {
    }

    public TaxonomyStore(string yaml)
    {
        var parsed = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build()
            .Deserialize<TaxonomyFile>(yaml)
            ?? throw new FormatException("taxonomy.yml is empty.");

        if (parsed.TumorTypes.Count == 0)
        {
            throw new FormatException("taxonomy.yml defines no tumor types.");
        }

        foreach (var type in parsed.TumorTypes)
        {
            if (string.IsNullOrWhiteSpace(type.Slug) || string.IsNullOrWhiteSpace(type.Label))
            {
                throw new FormatException($"taxonomy.yml entry '{type.Slug}' needs both slug and label.");
            }

            if (!_bySlug.TryAdd(type.Slug, type))
            {
                throw new FormatException($"taxonomy.yml has a duplicate slug '{type.Slug}'.");
            }
        }

        // Aliases second: a slug always wins over another entry's alias, and
        // two entries may never claim the same alias.
        foreach (var type in parsed.TumorTypes)
        {
            _aliasToSlug[type.Slug] = type.Slug;
        }

        foreach (var type in parsed.TumorTypes)
        {
            foreach (var rawAlias in type.Also)
            {
                var alias = rawAlias.Trim();
                if (alias.Length == 0)
                {
                    throw new FormatException($"taxonomy.yml entry '{type.Slug}' has a blank alias.");
                }

                if (_aliasToSlug.TryGetValue(alias, out var owner) && owner != type.Slug)
                {
                    throw new FormatException(
                        $"taxonomy.yml alias '{alias}' is claimed by both '{owner}' and '{type.Slug}'.");
                }

                _aliasToSlug[alias] = type.Slug;
            }
        }

        foreach (var type in parsed.TumorTypes)
        {
            if (type.Parent is null)
            {
                continue;
            }

            if (!_bySlug.ContainsKey(type.Parent))
            {
                throw new FormatException(
                    $"taxonomy.yml entry '{type.Slug}' names unknown parent '{type.Parent}'.");
            }

            // Walk to the root; a cycle would hang every later lookup.
            var seen = new HashSet<string>(StringComparer.Ordinal) { type.Slug };
            for (var cursor = type.Parent; cursor is not null; cursor = _bySlug[cursor].Parent)
            {
                if (!seen.Add(cursor))
                {
                    throw new FormatException(
                        $"taxonomy.yml has a parent cycle involving '{type.Slug}'.");
                }
            }
        }

        TumorTypes = parsed.TumorTypes;
    }

    /// <summary>
    /// Strict, case-sensitive: this answers "is this exact string safe to
    /// persist". Use <see cref="Resolve"/> to normalize first — validating
    /// leniently and then storing the raw tag would put 'GLIOMA' in the
    /// database, where the exact-match GIN index would never find it.
    /// </summary>
    public bool IsKnownSlug(string slug) => _bySlug.ContainsKey(slug);

    public TumorType? Find(string slug) => _bySlug.GetValueOrDefault(slug);

    public string LabelFor(string slug) => _bySlug.TryGetValue(slug, out var type) ? type.Label : slug;

    /// <summary>Maps a slug or a known alias (any casing) to its canonical slug; null if unknown.</summary>
    public string? Resolve(string slugOrAlias) =>
        string.IsNullOrWhiteSpace(slugOrAlias) ? null : _aliasToSlug.GetValueOrDefault(slugOrAlias.Trim());

    /// <summary>The slug plus every ancestor, most specific first.</summary>
    public IReadOnlyList<string> WithAncestors(string slug)
    {
        var chain = new List<string>();
        for (var cursor = Resolve(slug); cursor is not null; cursor = _bySlug[cursor].Parent)
        {
            chain.Add(cursor);
        }
        return chain;
    }

    /// <summary>
    /// Does an item carrying <paramref name="itemTags"/> belong under
    /// <paramref name="filterSlug"/>? True when the item is tagged with the
    /// filter itself or any descendant of it — so filtering "glioma" returns
    /// glioblastoma items. Items tagged all-brain-tumors match every filter.
    /// </summary>
    public bool Matches(IEnumerable<string> itemTags, string filterSlug)
    {
        var filter = Resolve(filterSlug);
        if (filter is null)
        {
            return false;
        }

        foreach (var tag in itemTags)
        {
            var resolved = Resolve(tag);
            if (resolved is null)
            {
                continue;
            }

            if (resolved == "all-brain-tumors" || WithAncestors(resolved).Contains(filter))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Keeps only tags that resolve to real taxonomy slugs, normalized to
    /// canonical casing, and reports the rest. Classifier output goes through
    /// here before it ever reaches the database; callers should log
    /// <see cref="TagFilterResult.Rejected"/> so a recurring unknown term
    /// becomes evidence for a new taxonomy entry instead of silent data loss.
    /// </summary>
    public TagFilterResult FilterTags(IEnumerable<string> tags)
    {
        var known = new List<string>();
        var rejected = new List<string>();

        foreach (var tag in tags)
        {
            var resolved = Resolve(tag);
            if (resolved is null)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    rejected.Add(tag.Trim());
                }
            }
            else if (!known.Contains(resolved))
            {
                known.Add(resolved);
            }
        }

        return new TagFilterResult([.. known], [.. rejected.Distinct()]);
    }
}
