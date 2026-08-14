using S1API.Entities;
using S1Lua.Hosting;

namespace S1Lua.Runtime;

internal sealed class S1ApiPlayerService
{
    internal PlayerSnapshot? Get()
    {
        Player? player = Player.Local;
        return player == null
            ? null
            : new PlayerSnapshot(
                player.Name,
                player.CurrentHealth,
                player.MaxHealth,
                player.IsDead,
                player.IsInVehicle,
                player.IsSleeping,
                player.IsArrested,
                RuntimeIdentifier.FromPascalCase(player.CurrentRegion.ToString()),
                new PositionSnapshot(player.Position.x, player.Position.y, player.Position.z));
    }

    internal IDisposable? SubscribeDied(Action callback)
    {
        Player? player = Player.Local;
        if (player == null)
            return null;
        player.OnDeath += callback;
        return new CallbackSubscription(() => player.OnDeath -= callback);
    }

    internal IDisposable? SubscribeRevived(Action callback)
    {
        Player? player = Player.Local;
        if (player == null)
            return null;
        player.OnRevive += callback;
        return new CallbackSubscription(() => player.OnRevive -= callback);
    }
}
