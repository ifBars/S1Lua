using S1API.Leveling;
using S1Lua.Hosting;

namespace S1Lua.Runtime;

internal sealed class S1ApiProgressionService
{
    internal ProgressSnapshot? Get()
    {
        if (!LevelManager.Exists)
            return null;

        return new ProgressSnapshot(
            RuntimeIdentifier.FromPascalCase(LevelManager.Rank.ToString()),
            LevelManager.Tier,
            LevelManager.XP,
            LevelManager.TotalXP,
            LevelManager.XPToNextTier);
    }

    internal bool AddXp(int amount)
    {
        if (!LevelManager.Exists)
            return false;

        LevelManager.AddXP(amount);
        return true;
    }
}
