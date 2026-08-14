using S1Lua.State;

namespace S1Lua.Hosting;

public enum S1LuaLogLevel
{
    Info,
    Warning,
    Error
}

public interface IS1LuaHost
{
    IModStateStore State { get; }

    void Log(S1LuaLogLevel level, string source, string message);

    bool RequestSave();

    NpcSnapshot? GetNpc(string npcId);

    bool ShowNpcText(string npcId, string text, double durationSeconds);

    bool SendNpcMessage(string npcId, string message);

    bool AddNpcRelationship(string npcId, double amount);

    bool UnlockNpc(string npcId);

    IDisposable? SubscribeNpcRelationship(string npcId, Action<double> callback);

    IDisposable? SubscribeNpcUnlocked(string npcId, Action<string, bool> callback);

    IDisposable? SubscribeNpcDied(string npcId, Action callback);

    GameTimeSnapshot? GetGameTime();

    WeatherSnapshot? GetWeather();

    MoneySnapshot GetMoney();

    void ChangeCash(double amount, bool visualizeChange, bool playCashSound);

    ProgressSnapshot? GetProgress();

    bool AddXp(int amount);

    PlayerSnapshot? GetPlayer();

    IDisposable? SubscribePlayerDied(Action callback);

    IDisposable? SubscribePlayerRevived(Action callback);

    IDisposable SubscribeTrashRecycled(Action<int> callback);

    IDisposable CreateMapMarker(MapMarkerRequest request);

    bool QueuePhoneCall(PhoneCallRequest request);

    QuestSnapshot? GetQuest(string questName);

    IDisposable? SubscribeQuestCompleted(string questName, Action callback);

    IDisposable? SubscribeQuestFailed(string questName, Action callback);
}
