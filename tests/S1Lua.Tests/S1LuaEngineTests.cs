using System.Diagnostics;
using S1Lua.Hosting;
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
    public void LoadsClothingDeclarationUsingPrimitiveOptions()
    {
        var engine = new S1LuaEngine(new TestHost());

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.streetwear", name = "Streetwear" }
            mod:item {
                id = "painted_cap",
                clone = "cap",
                name = "Painted Cap",
                clothing = {
                    slot = "head",
                    application = "accessory",
                    texture = "painted-cap.png",
                    colorable = false,
                    default_color = "black",
                    blocked_slots = { "eyes", "head" }
                }
            }
            """,
            ScriptPath("Streetwear"));

        Assert.True(result.Success, result.Error);
        ItemDeclaration item = Assert.Single(Assert.IsType<ScriptModSession>(result.Session).Items);
        ClothingDeclaration clothing = Assert.IsType<ClothingDeclaration>(item.Clothing);
        Assert.Equal("clothing", item.Category);
        Assert.Equal("head", clothing.Slot);
        Assert.Equal("accessory", clothing.Application);
        Assert.Equal("painted-cap.png", clothing.Texture);
        Assert.False(clothing.Colorable);
        Assert.Equal("black", clothing.DefaultColor);
        Assert.Equal(new[] { "eyes", "head" }, clothing.BlockedSlots);
    }

    [Fact]
    public void RejectsStandaloneClothingWithoutAnAsset()
    {
        var engine = new S1LuaEngine(new TestHost());

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.bad-clothes", name = "Bad Clothes" }
            mod:item { id = "hat", name = "Hat", clothing = { slot = "head" } }
            """,
            ScriptPath("BadClothes"));

        Assert.False(result.Success);
        Assert.Contains("must provide an 'asset'", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void RequiresAndCachesModulesInsideTheModFolder()
    {
        string directory = CreateScriptDirectory("Modules");
        string scriptPath = Path.Combine(directory, "mod.lua");
        File.WriteAllText(
            Path.Combine(directory, "messages.lua"),
            """
            module_loads = (module_loads or 0) + 1
            return { loads = module_loads, greeting = "Hello from a module" }
            """);
        var host = new TestHost();
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.modules", name = "Modules" }
            local first = mod:require("messages")
            local second = mod:require("messages.lua")
            assert(first == second)
            assert(first.loads == 1)
            assert(first.greeting == "Hello from a module")
            """,
            scriptPath);

        Assert.True(result.Success, result.Error);
    }

    [Fact]
    public void ModulePathsCannotEscapeTheModFolder()
    {
        string directory = CreateScriptDirectory("ModuleTraversal");
        var engine = new S1LuaEngine(new TestHost());

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.escape", name = "Escape" }
            mod:require("../outside.lua")
            """,
            Path.Combine(directory, "mod.lua"));

        Assert.False(result.Success);
        Assert.Contains("must stay inside this mod folder", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void DelayedAndRepeatingTimersAreBoundedAndCancellable()
    {
        var host = new TestHost();
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.timers", name = "Timers" }
            mod:after(2, function()
                mod:set("once", (mod:get("once", 0) or 0) + 1)
            end)

            local repeating
            repeating = mod:every(1, function()
                local count = (mod:get("repeats", 0) or 0) + 1
                mod:set("repeats", count)
                if count == 2 then
                    assert(mod:cancel(repeating))
                end
            end)
            """,
            ScriptPath("Timers"));

        Assert.True(result.Success, result.Error);
        engine.AdvanceTime(0.5);
        Assert.False(host.MemoryState.TryGet("alex.timers", "repeats", out _));

        engine.AdvanceTime(0.5);
        Assert.True(host.MemoryState.TryGet("alex.timers", "repeats", out StoredValue? first));
        Assert.Equal(1d, first?.Number);

        engine.AdvanceTime(1);
        Assert.True(host.MemoryState.TryGet("alex.timers", "once", out StoredValue? once));
        Assert.Equal(1d, once?.Number);
        Assert.True(host.MemoryState.TryGet("alex.timers", "repeats", out StoredValue? second));
        Assert.Equal(2d, second?.Number);

        engine.AdvanceTime(10);
        Assert.True(host.MemoryState.TryGet("alex.timers", "once", out once));
        Assert.Equal(1d, once?.Number);
        Assert.True(host.MemoryState.TryGet("alex.timers", "repeats", out second));
        Assert.Equal(2d, second?.Number);
    }

    [Fact]
    public void TimerIntervalsRejectPerFramePolling()
    {
        var engine = new S1LuaEngine(new TestHost());

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.fast-timer", name = "Fast Timer" }
            mod:every(0.001, function() end)
            """,
            ScriptPath("FastTimer"));

        Assert.False(result.Success);
        Assert.Contains("0.05 to 86400", result.Error, StringComparison.Ordinal);
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
    public void MoneySnapshotCashChangesAndBalanceEventUsePrimitiveValues()
    {
        var host = new TestHost
        {
            Money = new MoneySnapshot(125, 400, 900)
        };
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.cash-bonus", name = "Cash Bonus" }
            local money = s1.money()
            assert(money.cash == 125)
            assert(money.online == 400)
            assert(money.net_worth == 900)

            mod:change_cash(25)
            mod:change_cash(-10, false, true)
            mod:on("balance_changed", function()
                mod:set("last_cash", s1.money().cash)
            end)
            """,
            ScriptPath("CashBonus"));

        Assert.True(result.Success, result.Error);
        Assert.Equal((25d, true, false), host.CashChanges[0]);
        Assert.Equal((-10d, false, true), host.CashChanges[1]);

        engine.Dispatch("balance_changed");

        Assert.True(host.MemoryState.TryGet("alex.cash-bonus", "last_cash", out StoredValue? stored));
        Assert.Equal(125d, stored?.Number);
    }

    [Fact]
    public void CashChangeRejectsOutOfRangeAmounts()
    {
        var host = new TestHost();
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.bad-cash", name = "Bad Cash" }
            mod:change_cash(1000000001)
            """,
            ScriptPath("BadCash"));

        Assert.False(result.Success);
        Assert.Contains("finite number", result.Error, StringComparison.Ordinal);
        Assert.Empty(host.CashChanges);
    }

    [Fact]
    public void ProgressSnapshotXpAwardsAndEventsUsePrimitiveValues()
    {
        var host = new TestHost
        {
            Progress = new ProgressSnapshot("shot_caller", 4, 125, 5_000, 600)
        };
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.progress", name = "Progress" }
            local progress = s1.progress()
            assert(progress.rank == "shot_caller")
            assert(progress.tier == 4)
            assert(progress.xp == 125)
            assert(progress.total_xp == 5000)
            assert(progress.xp_to_next_tier == 600)
            assert(mod:add_xp(250))

            mod:on("xp_changed", function()
                mod:set("events", mod:get("events", 0) + 1)
            end)
            mod:on("rank_up", function()
                mod:set("events", mod:get("events", 0) + 1)
            end)
            """,
            ScriptPath("Progress"));

        Assert.True(result.Success, result.Error);
        Assert.Equal(250, Assert.Single(host.XpAwards));

        engine.Dispatch("xp_changed");
        engine.Dispatch("rank_up");

        Assert.True(host.MemoryState.TryGet("alex.progress", "events", out StoredValue? stored));
        Assert.Equal(2d, stored?.Number);
    }

    [Fact]
    public void ProgressApisReportUnavailableHostState()
    {
        var host = new TestHost();
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.no-progress", name = "No Progress" }
            assert(s1.progress() == nil)
            assert(mod:add_xp(10) == false)
            """,
            ScriptPath("NoProgress"));

        Assert.True(result.Success, result.Error);
        Assert.Empty(host.XpAwards);
    }

    [Fact]
    public void XpAwardsRejectFractionalAmounts()
    {
        var host = new TestHost
        {
            Progress = new ProgressSnapshot("street_rat", 1, 0, 0, 100)
        };
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.bad-xp", name = "Bad XP" }
            mod:add_xp(1.5)
            """,
            ScriptPath("BadXp"));

        Assert.False(result.Success);
        Assert.Contains("whole number", result.Error, StringComparison.Ordinal);
        Assert.Empty(host.XpAwards);
    }

    [Fact]
    public void PlayerSnapshotAndLifecycleEventsUsePrimitiveValues()
    {
        var host = new TestHost
        {
            Player = new PlayerSnapshot(
                "Player",
                75,
                100,
                false,
                true,
                false,
                false,
                "westville",
                new PositionSnapshot(12.5, 1, -8))
        };
        var engine = new S1LuaEngine(host);

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.player-status", name = "Player Status" }
            local player = s1.player()
            assert(player.name == "Player")
            assert(player.health == 75)
            assert(player.max_health == 100)
            assert(player.is_dead == false)
            assert(player.is_in_vehicle == true)
            assert(player.is_sleeping == false)
            assert(player.is_arrested == false)
            assert(player.region == "westville")
            assert(player.position.x == 12.5)
            assert(player.position.y == 1)
            assert(player.position.z == -8)

            mod:on("player_ready", function()
                mod:set("ready", true)
            end)

            mod:on("player_died", function()
                mod:set("last_event", "died")
            end)
            mod:on("player_revived", function()
                mod:set("last_event", "revived")
            end)
            """,
            ScriptPath("PlayerStatus"));

        Assert.True(result.Success, result.Error);

        engine.Dispatch("player_ready");
        Assert.True(host.MemoryState.TryGet("alex.player-status", "ready", out StoredValue? ready));
        Assert.True(ready?.Boolean);

        engine.Dispatch("player_died");
        Assert.True(host.MemoryState.TryGet("alex.player-status", "last_event", out StoredValue? died));
        Assert.Equal("died", died?.String);

        engine.Dispatch("player_revived");
        Assert.True(host.MemoryState.TryGet("alex.player-status", "last_event", out StoredValue? revived));
        Assert.Equal("revived", revived?.String);
    }

    [Fact]
    public void PlayerApiReturnsNilBeforeLocalPlayerSpawns()
    {
        var engine = new S1LuaEngine(new TestHost());

        ScriptLoadResult result = engine.LoadScript(
            """
            local mod = s1.mod { id = "alex.no-player", name = "No Player" }
            assert(s1.player() == nil)
            """,
            ScriptPath("NoPlayer"));

        Assert.True(result.Success, result.Error);
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

    [Fact]
    public void LoadDirectoryIgnoresReservedStarterFolders()
    {
        string root = Path.Combine(Path.GetTempPath(), "S1Lua.Tests", $"Authoring-{Guid.NewGuid():N}");
        string starter = Directory.CreateDirectory(Path.Combine(root, "_StarterMod")).FullName;
        string active = Directory.CreateDirectory(Path.Combine(root, "MyMod")).FullName;
        File.WriteAllText(
            Path.Combine(starter, "mod.lua"),
            "error('The packaged starter must not run')");
        File.WriteAllText(
            Path.Combine(active, "mod.lua"),
            "local mod = s1.mod { id = 'alex.active', name = 'Active' }");

        try
        {
            var engine = new S1LuaEngine(new TestHost());

            IReadOnlyList<ScriptLoadResult> results = engine.LoadDirectory(root);

            ScriptLoadResult loaded = Assert.Single(results);
            Assert.True(loaded.Success, loaded.Error);
            Assert.Equal("alex.active", Assert.Single(engine.Mods).Metadata?.Id);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string ScriptPath(string folder) =>
        Path.Combine(Path.GetTempPath(), "S1Lua.Tests", folder, "mod.lua");

    private static string CreateScriptDirectory(string folder)
    {
        string directory = Path.GetDirectoryName(ScriptPath($"{folder}-{Guid.NewGuid():N}"))!;
        Directory.CreateDirectory(directory);
        return directory;
    }
}
