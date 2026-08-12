using MelonLoader;
using MelonLoader.Utils;
using S1Lua.Generated;
using S1Lua.Scripting;

[assembly: MelonInfo(typeof(S1Lua.Runtime.S1LuaMod), "S1Lua", S1LuaBuild.Version, "Bars")]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: MelonAdditionalDependencies("S1API")]

namespace S1Lua.Runtime;

public sealed class S1LuaMod : MelonMod
{
    private S1LuaCoordinator? _coordinator;

    public override void OnInitializeMelon()
    {
        var host = new MelonS1LuaHost(LoggerInstance, S1LuaState.Store);
        var engine = new S1LuaEngine(host);
        string scriptsDirectory = Path.Combine(MelonEnvironment.ModsDirectory, "S1Lua");
        IReadOnlyList<ScriptLoadResult> results = engine.LoadDirectory(scriptsDirectory);

        _coordinator = new S1LuaCoordinator(engine, host, new S1ApiItemRegistrar(host));
        _coordinator.Attach();

        int loaded = results.Count(result => result.Success);
        int failed = results.Count - loaded;
        LoggerInstance.Msg($"Loaded {loaded} S1Lua mod(s) from {scriptsDirectory}.");
        if (failed > 0)
            LoggerInstance.Warning($"{failed} S1Lua mod(s) could not be loaded. See the errors above.");
        if (results.Count == 0)
        {
            LoggerInstance.Msg(
                "No Lua mods found. Create Mods/S1Lua/MyFirstMod/mod.lua or copy the included starter example.");
        }
    }

    public override void OnApplicationQuit()
    {
        _coordinator?.Detach();
        _coordinator = null;
    }
}
