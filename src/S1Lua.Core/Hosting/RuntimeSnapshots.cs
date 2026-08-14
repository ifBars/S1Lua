namespace S1Lua.Hosting;

public sealed record NpcSnapshot(
    string Id,
    string Name,
    string Region,
    double Relationship,
    bool IsUnlocked,
    bool IsDead);

public sealed record GameTimeSnapshot(
    string Day,
    int Time,
    string Formatted,
    int ElapsedDays,
    bool IsNight,
    bool IsSleeping);

public sealed record WeatherSnapshot(
    double Sunny,
    double Cloudy,
    double Rainy,
    double Stormy,
    double Snowy,
    double Foggy,
    double Windy,
    double Hail,
    double Sleet)
{
    public string Primary
    {
        get
        {
            (string Name, double Value)[] values =
            {
                ("sunny", Sunny),
                ("cloudy", Cloudy),
                ("rainy", Rainy),
                ("stormy", Stormy),
                ("snowy", Snowy),
                ("foggy", Foggy),
                ("windy", Windy),
                ("hail", Hail),
                ("sleet", Sleet)
            };

            return values
                .OrderByDescending(value => value.Value)
                .ThenBy(value => value.Name, StringComparer.Ordinal)
                .First()
                .Name;
        }
    }
}

public sealed record MoneySnapshot(
    double Cash,
    double Online,
    double NetWorth);

public sealed record ProgressSnapshot(
    string Rank,
    int Tier,
    int Xp,
    int TotalXp,
    double XpToNextTier);

public sealed record PositionSnapshot(double X, double Y, double Z);

public sealed record PlayerSnapshot(
    string Name,
    double Health,
    double MaxHealth,
    bool IsDead,
    bool IsInVehicle,
    bool IsSleeping,
    bool IsArrested,
    string Region,
    PositionSnapshot? Position = null);

public sealed record QuestSnapshot(string Id, string Title);

public sealed record MapMarkerRequest(
    string Id,
    string SourceDirectory,
    string Label,
    double? X,
    double? Y,
    double? Z,
    string? NpcId,
    string? Icon,
    string TextVisibility,
    bool Visible);

public sealed record PhoneCallRequest(
    string SourceDirectory,
    string Caller,
    string? NpcId,
    string? Icon,
    IReadOnlyList<string> Stages);
