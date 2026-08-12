using MoonSharp.Interpreter;

namespace S1Lua.Scripting;

internal enum NpcSubscriptionEvent
{
    RelationshipChanged,
    Unlocked,
    Died
}

internal sealed class NpcSubscription
{
    internal NpcSubscription(string npcId, NpcSubscriptionEvent eventName, DynValue callback)
    {
        NpcId = npcId;
        EventName = eventName;
        Callback = callback;
    }

    internal string NpcId { get; }
    internal NpcSubscriptionEvent EventName { get; }
    internal DynValue Callback { get; }
    internal IDisposable? Binding { get; set; }
}

internal enum QuestSubscriptionEvent
{
    Completed,
    Failed
}

internal sealed class QuestSubscription
{
    internal QuestSubscription(string questName, QuestSubscriptionEvent eventName, DynValue callback)
    {
        QuestName = questName;
        EventName = eventName;
        Callback = callback;
    }

    internal string QuestName { get; }
    internal QuestSubscriptionEvent EventName { get; }
    internal DynValue Callback { get; }
    internal IDisposable? Binding { get; set; }
}
