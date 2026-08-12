using System.Text.Json.Serialization;

namespace S1Lua.Generator;

public sealed class SurfaceDefinition
{
    public int SchemaVersion { get; init; }
    public string SurfaceVersion { get; init; } = string.Empty;
    public string S1ApiVersion { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<SurfaceTypeDefinition> Types { get; init; } = Array.Empty<SurfaceTypeDefinition>();
    public IReadOnlyList<BindingDefinition> Bindings { get; init; } = Array.Empty<BindingDefinition>();
    public IReadOnlyList<EventDefinition> Events { get; init; } = Array.Empty<EventDefinition>();
}

public sealed class SurfaceTypeDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<FieldDefinition> Fields { get; init; } = Array.Empty<FieldDefinition>();
}

public sealed class FieldDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string? Default { get; init; }
    public string Description { get; init; } = string.Empty;
}

public sealed class BindingDefinition
{
    public string Path { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string Handler { get; init; } = string.Empty;
    public string Signature { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<FieldDefinition> Parameters { get; init; } = Array.Empty<FieldDefinition>();
    public string? Returns { get; init; }
    public IReadOnlyList<string> Examples { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> S1ApiUids { get; init; } = Array.Empty<string>();
}

public sealed class EventDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string S1ApiUid { get; init; } = string.Empty;
}

public sealed record GeneratorOptions(
    string SurfacePath,
    string S1ApiDirectory,
    string RepositoryRoot,
    bool Check,
    string? UpdateS1ApiVersion,
    bool BumpSurfacePatch);

public sealed record GeneratedArtifact(string RelativePath, string Content);
