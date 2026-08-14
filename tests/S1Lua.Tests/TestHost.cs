using S1Lua.Hosting;
using S1Lua.State;

namespace S1Lua.Tests;

internal sealed class TestHost : IS1LuaHost
{
    internal List<(S1LuaLogLevel Level, string Source, string Message)> Messages { get; } = new();
    internal InMemoryModStateStore MemoryState { get; } = new();
    internal bool SaveAccepted { get; set; } = true;
    internal MoneySnapshot Money { get; set; } = new(0, 0, 0);
    internal List<(double Amount, bool Visualize, bool Sound)> CashChanges { get; } = new();
    internal ProgressSnapshot? Progress { get; set; }
    internal List<int> XpAwards { get; } = new();
    internal PlayerSnapshot? Player { get; set; }

    public IModStateStore State => MemoryState;

    public void Log(S1LuaLogLevel level, string source, string message) =>
        Messages.Add((level, source, message));

    public bool RequestSave() => SaveAccepted;

    public NpcSnapshot? GetNpc(string npcId) => null;

    public bool ShowNpcText(string npcId, string text, double durationSeconds) => false;

    public bool SendNpcMessage(string npcId, string message) => false;

    public bool AddNpcRelationship(string npcId, double amount) => false;

    public bool UnlockNpc(string npcId) => false;

    public IDisposable? SubscribeNpcRelationship(string npcId, Action<double> callback) => null;

    public IDisposable? SubscribeNpcUnlocked(string npcId, Action<string, bool> callback) => null;

    public IDisposable? SubscribeNpcDied(string npcId, Action callback) => null;

    public GameTimeSnapshot? GetGameTime() => null;

    public WeatherSnapshot? GetWeather() => null;

    public MoneySnapshot GetMoney() => Money;

    public void ChangeCash(double amount, bool visualizeChange, bool playCashSound) =>
        CashChanges.Add((amount, visualizeChange, playCashSound));

    public ProgressSnapshot? GetProgress() => Progress;

    public bool AddXp(int amount)
    {
        if (Progress == null)
            return false;
        XpAwards.Add(amount);
        return true;
    }

    public PlayerSnapshot? GetPlayer() => Player;

    public IDisposable? SubscribePlayerDied(Action callback) => null;

    public IDisposable? SubscribePlayerRevived(Action callback) => null;

    public IDisposable SubscribeTrashRecycled(Action<int> callback) => EmptySubscription.Instance;

    public IDisposable CreateMapMarker(MapMarkerRequest request) => EmptySubscription.Instance;

    public bool QueuePhoneCall(PhoneCallRequest request) => false;

    public QuestSnapshot? GetQuest(string questName) => null;

    public IDisposable? SubscribeQuestCompleted(string questName, Action callback) => null;

    public IDisposable? SubscribeQuestFailed(string questName, Action callback) => null;

    private sealed class EmptySubscription : IDisposable
    {
        internal static EmptySubscription Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
