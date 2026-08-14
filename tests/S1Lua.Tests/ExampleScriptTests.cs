using S1Lua.Scripting;

namespace S1Lua.Tests;

public sealed class ExampleScriptTests
{
    [Theory]
    [InlineData("examples/MyFirstMod/mod.lua")]
    [InlineData("examples/GoldenCuke/mod.lua")]
    [InlineData("examples/StatusWatcher/mod.lua")]
    [InlineData("examples/WelcomeBonus/mod.lua")]
    [InlineData("examples/RecycleForXp/mod.lua")]
    [InlineData("examples/ModularTimer/mod.lua")]
    public void ShippedBeginnerScriptsLoadAndDispatch(string relativePath)
    {
        string repositoryRoot = FindRepositoryRoot();
        string scriptPath = Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var host = new TestHost();
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(File.ReadAllText(scriptPath), scriptPath);
        engine.Dispatch("game_loaded");
        engine.AdvanceTime(1);

        Assert.True(result.Success, result.Error);
        Assert.DoesNotContain(host.Messages, message => message.Level == S1Lua.Hosting.S1LuaLogLevel.Error);
    }

    [Fact]
    public void RecycleForXpAwardsFiveXpPerTrashObject()
    {
        string repositoryRoot = FindRepositoryRoot();
        string scriptPath = Path.Combine(repositoryRoot, "examples", "RecycleForXp", "mod.lua");
        var host = new TestHost
        {
            Progress = new S1Lua.Hosting.ProgressSnapshot("street_rat", 1, 0, 0, 100)
        };
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(File.ReadAllText(scriptPath), scriptPath);
        engine.Dispatch("trash_recycled", 4);

        Assert.True(result.Success, result.Error);
        Assert.Equal(20, Assert.Single(host.XpAwards));
    }

    private static string FindRepositoryRoot()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "surface", "s1lua.surface.json")))
                    return directory.FullName;
                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the S1Lua repository root.");
    }
}
