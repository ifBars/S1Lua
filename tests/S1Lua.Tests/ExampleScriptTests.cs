using S1Lua.Scripting;

namespace S1Lua.Tests;

public sealed class ExampleScriptTests
{
    [Theory]
    [InlineData("templates/MyFirstMod/mod.lua")]
    [InlineData("examples/GoldenCuke/mod.lua")]
    public void ShippedBeginnerScriptsLoadAndDispatch(string relativePath)
    {
        string repositoryRoot = FindRepositoryRoot();
        string scriptPath = Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var host = new TestHost();
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(File.ReadAllText(scriptPath), scriptPath);
        engine.Dispatch("game_loaded");

        Assert.True(result.Success, result.Error);
        Assert.DoesNotContain(host.Messages, message => message.Level == S1Lua.Hosting.S1LuaLogLevel.Error);
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
