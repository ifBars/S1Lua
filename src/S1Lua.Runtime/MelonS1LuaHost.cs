using MelonLoader;
using S1API.Internal.Abstraction;
using S1Lua.Hosting;
using S1Lua.State;

namespace S1Lua.Runtime;

internal sealed class MelonS1LuaHost : IS1LuaHost
{
    private readonly MelonLogger.Instance _logger;
    private readonly S1ApiMapMarkerService _mapMarkers = new();
    private readonly S1ApiMoneyService _money = new();
    private readonly S1ApiNpcService _npcs = new();
    private readonly S1ApiPhoneCallService _phoneCalls = new();
    private readonly S1ApiPlayerService _player = new();
    private readonly S1ApiProgressionService _progression = new();
    private readonly S1ApiRecyclingService _recycling = new();
    private readonly S1ApiQuestService _quests = new();
    private readonly S1ApiWorldService _world = new();

    internal MelonS1LuaHost(MelonLogger.Instance logger, InMemoryModStateStore state)
    {
        _logger = logger;
        State = state;
    }

    public IModStateStore State { get; }

    public void Log(S1LuaLogLevel level, string source, string message)
    {
        string formatted = $"[{source}] {message}";
        switch (level)
        {
            case S1LuaLogLevel.Info:
                _logger.Msg(formatted);
                break;
            case S1LuaLogLevel.Warning:
                _logger.Warning(formatted);
                break;
            case S1LuaLogLevel.Error:
                _logger.Error(formatted);
                break;
            default:
                _logger.Msg(formatted);
                break;
        }
    }

    public bool RequestSave() => Saveable.RequestGameSave();

    public NpcSnapshot? GetNpc(string npcId) => _npcs.Get(npcId);

    public bool ShowNpcText(string npcId, string text, double durationSeconds) =>
        _npcs.ShowText(npcId, text, durationSeconds);

    public bool SendNpcMessage(string npcId, string message) => _npcs.SendMessage(npcId, message);

    public bool AddNpcRelationship(string npcId, double amount) => _npcs.AddRelationship(npcId, amount);

    public bool UnlockNpc(string npcId) => _npcs.Unlock(npcId);

    public IDisposable? SubscribeNpcRelationship(string npcId, Action<double> callback) =>
        _npcs.SubscribeRelationship(npcId, callback);

    public IDisposable? SubscribeNpcUnlocked(string npcId, Action<string, bool> callback) =>
        _npcs.SubscribeUnlocked(npcId, callback);

    public IDisposable? SubscribeNpcDied(string npcId, Action callback) =>
        _npcs.SubscribeDied(npcId, callback);

    public GameTimeSnapshot? GetGameTime() => _world.GetTime();

    public WeatherSnapshot? GetWeather() => _world.GetWeather();

    public MoneySnapshot GetMoney() => _money.Get();

    public void ChangeCash(double amount, bool visualizeChange, bool playCashSound) =>
        _money.ChangeCash(amount, visualizeChange, playCashSound);

    public ProgressSnapshot? GetProgress() => _progression.Get();

    public bool AddXp(int amount) => _progression.AddXp(amount);

    public PlayerSnapshot? GetPlayer() => _player.Get();

    public IDisposable? SubscribePlayerDied(Action callback) => _player.SubscribeDied(callback);

    public IDisposable? SubscribePlayerRevived(Action callback) => _player.SubscribeRevived(callback);

    public IDisposable SubscribeTrashRecycled(Action<int> callback) => _recycling.Subscribe(callback);

    public IDisposable CreateMapMarker(MapMarkerRequest request) => _mapMarkers.Create(request);

    public bool QueuePhoneCall(PhoneCallRequest request) => _phoneCalls.Queue(request);

    public QuestSnapshot? GetQuest(string questName) => _quests.Get(questName);

    public IDisposable? SubscribeQuestCompleted(string questName, Action callback) =>
        _quests.SubscribeCompleted(questName, callback);

    public IDisposable? SubscribeQuestFailed(string questName, Action callback) =>
        _quests.SubscribeFailed(questName, callback);
}
