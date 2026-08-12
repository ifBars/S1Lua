using S1API.Entities;
using S1Lua.Hosting;

namespace S1Lua.Runtime;

internal sealed class S1ApiNpcService
{
    internal NpcSnapshot? Get(string npcId)
    {
        NPC? npc = NPC.Get(npcId);
        return npc == null
            ? null
            : new NpcSnapshot(
                npc.ID,
                npc.FullName,
                npc.Region.ToString().ToLowerInvariant(),
                npc.Relationship.Normalized,
                npc.Relationship.IsUnlocked,
                npc.IsDead);
    }

    internal bool ShowText(string npcId, string text, double durationSeconds)
    {
        NPC? npc = NPC.Get(npcId);
        if (npc == null)
            return false;
        npc.Dialogue.ShowWorldText(text, (float)durationSeconds);
        return true;
    }

    internal bool SendMessage(string npcId, string message)
    {
        NPC? npc = NPC.Get(npcId);
        if (npc == null)
            return false;
        npc.Messaging.SendTextMessage(message);
        return true;
    }

    internal bool AddRelationship(string npcId, double amount)
    {
        NPC? npc = NPC.Get(npcId);
        if (npc == null)
            return false;
        npc.Relationship.Add((float)amount);
        return true;
    }

    internal bool Unlock(string npcId)
    {
        NPC? npc = NPC.Get(npcId);
        if (npc == null)
            return false;
        npc.Relationship.Unlock();
        return true;
    }

    internal IDisposable? SubscribeRelationship(string npcId, Action<double> callback)
    {
        NPC? npc = NPC.Get(npcId);
        if (npc == null)
            return null;
        Action<float> handler = value => callback(value);
        npc.Relationship.OnChanged += handler;
        return new CallbackSubscription(() => npc.Relationship.OnChanged -= handler);
    }

    internal IDisposable? SubscribeUnlocked(string npcId, Action<string, bool> callback)
    {
        NPC? npc = NPC.Get(npcId);
        if (npc == null)
            return null;
        Action<NPCRelationship.UnlockType, bool> handler =
            (unlockType, notify) => callback(unlockType.ToString().ToLowerInvariant(), notify);
        npc.Relationship.OnUnlocked += handler;
        return new CallbackSubscription(() => npc.Relationship.OnUnlocked -= handler);
    }

    internal IDisposable? SubscribeDied(string npcId, Action callback)
    {
        NPC? npc = NPC.Get(npcId);
        if (npc == null)
            return null;
        npc.OnDeath += callback;
        return new CallbackSubscription(() => npc.OnDeath -= callback);
    }
}
