namespace S1Lua.Model;

public sealed record ModMetadata(
    string Id,
    string Name,
    string Version,
    string? Author,
    string? Description);
