using System.Diagnostics;
using S1Lua.Model;
using S1Lua.Scripting;
using S1Lua.State;

namespace S1Lua.Tests;

public sealed class S1LuaEngineTests
{
    [Fact]
    public void LoadsBeginnerItemDeclarationWithoutClrTypes()
    {
        var host = new TestHost();
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod {
                id = "alex.golden-cuke",
                name = "Golden Cuke",
                author = "Alex"
            }

            local item_id = mod:item {
                id = "golden_cuke",
                clone = "cuke",
                name = "Golden Cuke",
                price = 250,
                stack = 20,
                legal = true,
                shops = "compatible"
            }

            assert(item_id == "alex.golden-cuke:golden_cuke")
            """,
            ScriptPath("GoldenCuke"));

        Assert.True(result.Success, result.Error);
        ScriptModSession session = Assert.IsType<ScriptModSession>(result.Session);
        ItemDeclaration item = Assert.Single(session.Items);
        Assert.Equal("alex.golden-cuke:golden_cuke", item.Id);
        Assert.Equal("cuke", item.CloneSourceId);
        Assert.Equal(250d, item.Price);
        Assert.Equal(ShopSelectionKind.Compatible, item.Shops.Kind);
    }

    [Fact]
    public void HardSandboxDoesNotExposeIoProcessOrClrAccess()
    {
        var engine = new S1LuaEngine(new TestHost());

        ScriptLoadResult result = engine.LoadScript(
            """
            assert(io == nil)
            assert(debug == nil)
            assert(clr == nil)
            assert(os == nil or os.execute == nil)
            local mod = s1.mod { id = "alex.sandbox", name = "Sandbox" }
            """,
            ScriptPath("Sandbox"));

        Assert.True(result.Success, result.Error);
    }

    [Fact]
    public void EventsCanReadAndWriteIsolatedScalarState()
    {
        var host = new TestHost();
        var engine = new S1LuaEngine(host);
        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.counter", name = "Counter" }
            mod:on("game_loaded", function()
                mod:set("loads", mod:get("loads", 0) + 1)
            end)
            """,
            ScriptPath("Counter"));

        Assert.True(result.Success, result.Error);
        engine.Dispatch("game_loaded");
        engine.Dispatch("game_loaded");

        Assert.True(host.MemoryState.TryGet("alex.counter", "loads", out StoredValue? stored));
        Assert.NotNull(stored);
        Assert.Equal(StoredValueKind.Number, stored.Kind);
        Assert.Equal(2d, stored.Number);
    }

    [Fact]
    public void CallbackFailureDoesNotBlockLaterCallbacks()
    {
        var host = new TestHost();
        var engine = new S1LuaEngine(host);
        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.callbacks", name = "Callbacks" }
            mod:on("game_loaded", function() error("first callback failed") end)
            mod:on("game_loaded", function() mod:set("second_ran", true) end)
            """,
            ScriptPath("Callbacks"));

        Assert.True(result.Success, result.Error);
        engine.Dispatch("game_loaded");

        Assert.Contains(host.Messages, message =>
            message.Level == S1Lua.Hosting.S1LuaLogLevel.Error &&
            message.Message.Contains("first callback failed", StringComparison.Ordinal));
        Assert.True(host.MemoryState.TryGet("alex.callbacks", "second_ran", out StoredValue? stored));
        Assert.True(stored?.Boolean);
    }

    [Fact]
    public void InvalidBeginnerInputProducesActionableError()
    {
        var engine = new S1LuaEngine(new TestHost());

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "Alex Has Spaces", name = "Broken" }
            """,
            ScriptPath("Broken"));

        Assert.False(result.Success);
        Assert.Contains("lowercase letters", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void StartupInfiniteLoopIsStoppedByExecutionBudget()
    {
        var engine = new S1LuaEngine(new TestHost());
        var stopwatch = Stopwatch.StartNew();

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.loop", name = "Loop" }
            while true do end
            """,
            ScriptPath("Loop"));

        Assert.False(result.Success);
        Assert.Contains("loop that never ends", result.Error, StringComparison.Ordinal);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void DuplicateModIdsAreRejected()
    {
        var engine = new S1LuaEngine(new TestHost());
        Assert.True(engine.LoadScript(
            "local mod = s1.mod { id = \"alex.same\", name = \"First\" }",
            ScriptPath("First")).Success);

        ScriptLoadResult duplicate = engine.LoadScript(
            "local mod = s1.mod { id = \"alex.same\", name = \"Second\" }",
            ScriptPath("Second"));

        Assert.False(duplicate.Success);
        Assert.Contains("already uses mod id", duplicate.Error, StringComparison.Ordinal);
    }

    private static string ScriptPath(string folder) =>
        Path.Combine(Path.GetTempPath(), "S1Lua.Tests", folder, "mod.lua");
}
