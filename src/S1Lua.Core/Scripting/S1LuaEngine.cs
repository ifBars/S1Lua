using MoonSharp.Interpreter;
using S1Lua.Generated;
using S1Lua.Hosting;

namespace S1Lua.Scripting;

public sealed class S1LuaEngine
{
    private readonly Dictionary<string, ScriptModSession> _mods = new(StringComparer.Ordinal);
    private readonly IS1LuaHost _host;

    public S1LuaEngine(IS1LuaHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public IReadOnlyCollection<ScriptModSession> Mods => _mods.Values;

    public ScriptLoadResult LoadScript(string source, string scriptPath)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrWhiteSpace(scriptPath))
            throw new ArgumentException("Script path cannot be empty.", nameof(scriptPath));

        var script = new Script(CoreModules.Preset_HardSandbox);
        var session = new ScriptModSession(script, scriptPath, _host);
        var bindings = new BeginnerApiBindings(_host, session);
        GeneratedSurface.RegisterGlobals(script, bindings);
        script.Options.DebugPrint = message =>
            _host.Log(S1LuaLogLevel.Info, session.DisplayName, message);

        try
        {
            ScriptExecutionBudget.RunSource(script, source, session.ScriptPath);
            if (session.Metadata == null)
                throw new ScriptRuntimeException("This file does not declare a mod. Start with: local mod = s1.mod { id = \"yourname.mod-name\", name = \"Mod Name\" }");
            if (_mods.ContainsKey(session.Metadata.Id))
                throw new ScriptRuntimeException($"Another script already uses mod id '{session.Metadata.Id}'. Every mod needs a unique ID.");

            _mods.Add(session.Metadata.Id, session);
            _host.Log(
                S1LuaLogLevel.Info,
                session.Metadata.Name,
                $"Loaded {session.Items.Count} item declaration(s) from {Path.GetFileName(session.ScriptPath)}.");
            return ScriptLoadResult.Loaded(session);
        }
        catch (SyntaxErrorException ex)
        {
            return Failed(session, ex.DecoratedMessage ?? ex.Message);
        }
        catch (ScriptRuntimeException ex)
        {
            return Failed(session, ex.DecoratedMessage ?? ex.Message);
        }
        catch (Exception ex)
        {
            return Failed(session, ex.Message);
        }
    }

    public IReadOnlyList<ScriptLoadResult> LoadDirectory(string rootDirectory)
    {
        string root = Path.GetFullPath(rootDirectory);
        Directory.CreateDirectory(root);
        var results = new List<ScriptLoadResult>();
        foreach (string directory in Directory.EnumerateDirectories(root).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            string entryPoint = Path.Combine(directory, "mod.lua");
            if (!File.Exists(entryPoint))
                continue;

            try
            {
                results.Add(LoadScript(File.ReadAllText(entryPoint), entryPoint));
            }
            catch (Exception ex)
            {
                string error = $"Could not read {entryPoint}: {ex.Message}";
                _host.Log(S1LuaLogLevel.Error, Path.GetFileName(directory), error);
                results.Add(ScriptLoadResult.Failed(error));
            }
        }
        return results;
    }

    public void Dispatch(string eventName, params object?[] arguments)
    {
        foreach (ScriptModSession session in _mods.Values.ToArray())
            session.Dispatch(eventName, arguments.Select(ToDynValue).ToArray());
    }

    public void BindRuntimeSubscriptions()
    {
        foreach (ScriptModSession session in _mods.Values.ToArray())
            session.BindRuntimeSubscriptions();
    }

    public void UnbindRuntimeSubscriptions()
    {
        foreach (ScriptModSession session in _mods.Values.ToArray())
            session.UnbindRuntimeSubscriptions();
    }

    private static DynValue ToDynValue(object? value)
    {
        return value switch
        {
            null => DynValue.Nil,
            string text => DynValue.NewString(text),
            bool boolean => DynValue.NewBoolean(boolean),
            byte number => DynValue.NewNumber(number),
            short number => DynValue.NewNumber(number),
            int number => DynValue.NewNumber(number),
            long number => DynValue.NewNumber(number),
            float number => DynValue.NewNumber(number),
            double number => DynValue.NewNumber(number),
            _ => throw new ArgumentException($"Unsupported Lua event argument type: {value.GetType().FullName}.")
        };
    }

    private ScriptLoadResult Failed(ScriptModSession session, string error)
    {
        _host.Log(S1LuaLogLevel.Error, session.DisplayName, error);
        return ScriptLoadResult.Failed(error);
    }
}
