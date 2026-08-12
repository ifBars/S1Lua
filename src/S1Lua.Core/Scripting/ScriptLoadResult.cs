namespace S1Lua.Scripting;

public sealed record ScriptLoadResult(bool Success, ScriptModSession? Session, string? Error)
{
    public static ScriptLoadResult Loaded(ScriptModSession session) => new(true, session, null);
    public static ScriptLoadResult Failed(string error) => new(false, null, error);
}
