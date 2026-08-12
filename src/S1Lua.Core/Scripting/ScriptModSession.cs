using MoonSharp.Interpreter;
using S1Lua.Hosting;
using S1Lua.Model;

namespace S1Lua.Scripting;

public sealed class ScriptModSession
{
    private readonly Dictionary<string, List<DynValue>> _callbacks = new(StringComparer.Ordinal);
    private readonly HashSet<string> _itemIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _markerIds = new(StringComparer.Ordinal);
    private readonly List<ItemDeclaration> _items = new();
    private readonly List<MapMarkerRequest> _markers = new();
    private readonly List<NpcSubscription> _npcSubscriptions = new();
    private readonly List<QuestSubscription> _questSubscriptions = new();
    private readonly IS1LuaHost _host;
    private bool _runtimeReady;

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
}
