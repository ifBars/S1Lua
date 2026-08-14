using MoonSharp.Interpreter;
using S1Lua.Hosting;
using S1Lua.Model;

namespace S1Lua.Scripting;

public sealed class ScriptModSession
{
    private const int MaximumActiveTimers = 128;
    private readonly Dictionary<string, List<DynValue>> _callbacks = new(StringComparer.Ordinal);
    private readonly HashSet<string> _itemIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _markerIds = new(StringComparer.Ordinal);
    private readonly List<ItemDeclaration> _items = new();
    private readonly List<MapMarkerRequest> _markers = new();
    private readonly List<NpcSubscription> _npcSubscriptions = new();
    private readonly List<QuestSubscription> _questSubscriptions = new();
    private readonly Dictionary<string, DynValue> _modules = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _loadingModules = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, ScriptTimer> _timers = new();
    private readonly IS1LuaHost _host;
    private bool _runtimeReady;
    private int _nextTimerId = 1;

    internal ScriptModSession(Script script, string scriptPath, IS1LuaHost host)
    {
        Script = script;
        ScriptPath = Path.GetFullPath(scriptPath);
        SourceDirectory = Path.GetDirectoryName(ScriptPath) ?? Directory.GetCurrentDirectory();
        _host = host;
    }

    internal Script Script { get; }
    public string ScriptPath { get; }
    public string SourceDirectory { get; }
    public ModMetadata? Metadata { get; private set; }
    public IReadOnlyList<ItemDeclaration> Items => _items;
    public IReadOnlyList<MapMarkerRequest> Markers => _markers;

    internal void Initialize(ModMetadata metadata)
    {
        if (Metadata != null)
            throw new ScriptRuntimeException("s1.mod can only be called once in each mod.lua file.");
        Metadata = metadata;
    }

    internal void AddItem(ItemDeclaration item)
    {
        if (!_itemIds.Add(item.Id))
            throw new ScriptRuntimeException($"mod:item: duplicate item id '{item.LocalId}'.");
        _items.Add(item);
    }

    internal void AddMarker(MapMarkerRequest marker)
    {
        if (!_markerIds.Add(marker.Id))
            throw new ScriptRuntimeException($"mod:marker: duplicate marker id '{marker.Id}'.");
        _markers.Add(marker);
    }

    internal DynValue RequireModule(string moduleName)
    {
        string modulePath = ResolveModulePath(moduleName);
        if (_modules.TryGetValue(modulePath, out DynValue? cached))
            return cached;
        if (!_loadingModules.Add(modulePath))
            throw new ScriptRuntimeException($"mod:require: circular module import detected for '{moduleName}'.");

        try
        {
            if (!File.Exists(modulePath))
                throw new ScriptRuntimeException($"mod:require: module '{moduleName}' was not found in this mod folder.");

            string source = File.ReadAllText(modulePath);
            DynValue result = ScriptExecutionBudget.RunSource(Script, source, modulePath);
            if (result.IsNil())
                result = DynValue.NewBoolean(true);
            _modules.Add(modulePath, result);
            return result;
        }
        finally
        {
            _loadingModules.Remove(modulePath);
        }
    }

    internal int ScheduleTimer(double seconds, bool repeat, DynValue callback)
    {
        if (_timers.Count >= MaximumActiveTimers)
            throw new ScriptRuntimeException($"A mod may have at most {MaximumActiveTimers} active timers.");

        int id = _nextTimerId++;
        _timers.Add(id, new ScriptTimer(seconds, repeat, callback));
        return id;
    }

    internal bool CancelTimer(int id) => _timers.Remove(id);

    internal void AdvanceTimers(double elapsedSeconds)
    {
        foreach (int id in _timers.Keys.ToArray())
        {
            if (!_timers.TryGetValue(id, out ScriptTimer? timer))
                continue;

            timer.RemainingSeconds -= elapsedSeconds;
            if (timer.RemainingSeconds > 0)
                continue;

            if (timer.Repeat)
            {
                timer.RemainingSeconds += timer.IntervalSeconds;
                if (timer.RemainingSeconds <= 0)
                    timer.RemainingSeconds = timer.IntervalSeconds;
            }
            else
            {
                _timers.Remove(id);
            }

            Invoke(timer.Callback, $"timer {id}");
        }
    }

    internal void CancelAllTimers() => _timers.Clear();

    internal void Subscribe(string eventName, DynValue callback)
    {
        if (!_callbacks.TryGetValue(eventName, out List<DynValue>? callbacks))
        {
            callbacks = new List<DynValue>();
            _callbacks.Add(eventName, callbacks);
        }
        callbacks.Add(callback);
    }

    internal void SubscribeNpc(string npcId, NpcSubscriptionEvent eventName, DynValue callback)
    {
        var subscription = new NpcSubscription(npcId, eventName, callback);
        _npcSubscriptions.Add(subscription);
        if (_runtimeReady)
            Bind(subscription);
    }

    internal void SubscribeQuest(string questName, QuestSubscriptionEvent eventName, DynValue callback)
    {
        var subscription = new QuestSubscription(questName, eventName, callback);
        _questSubscriptions.Add(subscription);
        if (_runtimeReady)
            Bind(subscription);
    }

    public void BindRuntimeSubscriptions()
    {
        UnbindRuntimeSubscriptions();
        _runtimeReady = true;
        foreach (NpcSubscription subscription in _npcSubscriptions)
            Bind(subscription);
        foreach (QuestSubscription subscription in _questSubscriptions)
            Bind(subscription);
    }

    public void UnbindRuntimeSubscriptions()
    {
        _runtimeReady = false;
        foreach (NpcSubscription subscription in _npcSubscriptions)
        {
            subscription.Binding?.Dispose();
            subscription.Binding = null;
        }
        foreach (QuestSubscription subscription in _questSubscriptions)
        {
            subscription.Binding?.Dispose();
            subscription.Binding = null;
        }
    }

    public void Dispatch(string eventName, params DynValue[] arguments)
    {
        if (!_callbacks.TryGetValue(eventName, out List<DynValue>? callbacks))
            return;

        foreach (DynValue callback in callbacks.ToArray())
        {
            try
            {
                ScriptExecutionBudget.RunFunction(
                    Script,
                    callback,
                    $"{DisplayName} '{eventName}' callback",
                    arguments);
            }
            catch (ScriptRuntimeException ex)
            {
                _host.Log(S1LuaLogLevel.Error, DisplayName, ex.DecoratedMessage ?? ex.Message);
            }
            catch (Exception ex)
            {
                _host.Log(S1LuaLogLevel.Error, DisplayName, ex.Message);
            }
        }
    }

    private void Bind(NpcSubscription subscription)
    {
        subscription.Binding?.Dispose();
        subscription.Binding = subscription.EventName switch
        {
            NpcSubscriptionEvent.RelationshipChanged => _host.SubscribeNpcRelationship(
                subscription.NpcId,
                value => Invoke(subscription.Callback, $"NPC '{subscription.NpcId}' relationship_changed", DynValue.NewNumber(value))),
            NpcSubscriptionEvent.Unlocked => _host.SubscribeNpcUnlocked(
                subscription.NpcId,
                (unlockType, notify) => Invoke(
                    subscription.Callback,
                    $"NPC '{subscription.NpcId}' unlocked",
                    DynValue.NewString(unlockType),
                    DynValue.NewBoolean(notify))),
            NpcSubscriptionEvent.Died => _host.SubscribeNpcDied(
                subscription.NpcId,
                () => Invoke(subscription.Callback, $"NPC '{subscription.NpcId}' died")),
            _ => null
        };
    }

    private void Bind(QuestSubscription subscription)
    {
        subscription.Binding?.Dispose();
        subscription.Binding = subscription.EventName switch
        {
            QuestSubscriptionEvent.Completed => _host.SubscribeQuestCompleted(
                subscription.QuestName,
                () => Invoke(subscription.Callback, $"quest '{subscription.QuestName}' completed")),
            QuestSubscriptionEvent.Failed => _host.SubscribeQuestFailed(
                subscription.QuestName,
                () => Invoke(subscription.Callback, $"quest '{subscription.QuestName}' failed")),
            _ => null
        };
    }

    private void Invoke(DynValue callback, string label, params DynValue[] arguments)
    {
        try
        {
            ScriptExecutionBudget.RunFunction(Script, callback, $"{DisplayName} {label} callback", arguments);
        }
        catch (ScriptRuntimeException ex)
        {
            _host.Log(S1LuaLogLevel.Error, DisplayName, ex.DecoratedMessage ?? ex.Message);
        }
        catch (Exception ex)
        {
            _host.Log(S1LuaLogLevel.Error, DisplayName, ex.Message);
        }
    }

    public string DisplayName => Metadata?.Name ?? Path.GetFileName(SourceDirectory);

    private string ResolveModulePath(string moduleName)
    {
        string trimmed = moduleName.Trim();
        if (trimmed.Length > 200 || Path.IsPathRooted(trimmed))
            throw new ScriptRuntimeException("mod:require: module paths must be short relative paths inside this mod folder.");

        string relativePath = trimmed
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        if (string.IsNullOrEmpty(Path.GetExtension(relativePath)))
            relativePath += ".lua";
        if (!string.Equals(Path.GetExtension(relativePath), ".lua", StringComparison.OrdinalIgnoreCase))
            throw new ScriptRuntimeException("mod:require: modules must be Lua files.");

        string fullPath = Path.GetFullPath(Path.Combine(SourceDirectory, relativePath));
        string rootPrefix = SourceDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                            Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new ScriptRuntimeException("mod:require: module paths must stay inside this mod folder.");
        if (string.Equals(fullPath, ScriptPath, StringComparison.OrdinalIgnoreCase))
            throw new ScriptRuntimeException("mod:require: mod.lua cannot import itself.");
        return fullPath;
    }

    private sealed class ScriptTimer
    {
        internal ScriptTimer(double intervalSeconds, bool repeat, DynValue callback)
        {
            IntervalSeconds = intervalSeconds;
            RemainingSeconds = intervalSeconds;
            Repeat = repeat;
            Callback = callback;
        }

        internal double IntervalSeconds { get; }
        internal double RemainingSeconds { get; set; }
        internal bool Repeat { get; }
        internal DynValue Callback { get; }
    }
}
