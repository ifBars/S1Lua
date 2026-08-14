using S1API.Lifecycle;
using S1API.GameTime;
using S1API.Weather;
using S1API.Money;
using S1API.Leveling;
using S1API.Entities;
using S1Lua.Hosting;
using S1Lua.Model;
using S1Lua.Scripting;

namespace S1Lua.Runtime;

internal sealed class S1LuaCoordinator
{
    private readonly S1LuaEngine _engine;
    private readonly IS1LuaHost _host;
    private readonly S1ApiItemRegistrar _items;
    private readonly Dictionary<string, S1API.Items.ItemDefinition> _registeredItems = new(StringComparer.Ordinal);
    private readonly List<IDisposable> _mapMarkers = new();
    private IDisposable? _playerDiedSubscription;
    private IDisposable? _playerRevivedSubscription;
    private IDisposable? _trashRecycledSubscription;
    private bool _attached;
    private bool _dispatchingBalanceChanged;
    private bool _dispatchingXpChanged;
    private bool _dispatchingRankUp;
    private bool _playerReadyDispatched;

    internal S1LuaCoordinator(S1LuaEngine engine, IS1LuaHost host, S1ApiItemRegistrar items)
    {
        _engine = engine;
        _host = host;
        _items = items;
    }

    internal void Attach()
    {
        if (_attached)
            return;
        _attached = true;
        GameLifecycle.OnPreLoad += OnPreLoad;
        GameLifecycle.OnLoadComplete += OnLoadComplete;
        GameLifecycle.OnPreSceneChange += OnPreSceneChange;
        GameLifecycle.OnSaveStart += OnSaveStart;
        GameLifecycle.OnSaveComplete += OnSaveComplete;
        TimeManager.OnHourPass += OnHourPassed;
        TimeManager.OnDayPass += OnDayPassed;
        TimeManager.OnWeekPass += OnWeekPassed;
        TimeManager.OnSleepStart += OnSleepStarted;
        TimeManager.OnSleepEnd += OnSleepEnded;
        WeatherManager.OnWeatherChanged += OnWeatherChanged;
        Money.OnBalanceChanged += OnBalanceChanged;
        LevelManager.OnXPChanged += OnXpChanged;
        LevelManager.OnRankUp += OnRankUp;
        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;
        _trashRecycledSubscription = _host.SubscribeTrashRecycled(OnTrashRecycled);
    }

    internal void Detach()
    {
        if (!_attached)
            return;
        _attached = false;
        GameLifecycle.OnPreLoad -= OnPreLoad;
        GameLifecycle.OnLoadComplete -= OnLoadComplete;
        GameLifecycle.OnPreSceneChange -= OnPreSceneChange;
        GameLifecycle.OnSaveStart -= OnSaveStart;
        GameLifecycle.OnSaveComplete -= OnSaveComplete;
#pragma warning disable CS8601 // S1API exposes nullable Action fields; removing a handler can legitimately leave null.
        TimeManager.OnHourPass -= OnHourPassed;
        TimeManager.OnDayPass -= OnDayPassed;
        TimeManager.OnWeekPass -= OnWeekPassed;
        TimeManager.OnSleepStart -= OnSleepStarted;
        TimeManager.OnSleepEnd -= OnSleepEnded;
#pragma warning restore CS8601
        WeatherManager.OnWeatherChanged -= OnWeatherChanged;
        Money.OnBalanceChanged -= OnBalanceChanged;
        LevelManager.OnXPChanged -= OnXpChanged;
        LevelManager.OnRankUp -= OnRankUp;
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
        _trashRecycledSubscription?.Dispose();
        _trashRecycledSubscription = null;
        _engine.UnbindRuntimeSubscriptions();
        _engine.CancelTimers();
        DisposePlayerSubscriptions();
        DisposeMapMarkers();
        _registeredItems.Clear();
        _playerReadyDispatched = false;
    }

    internal void Update(double elapsedSeconds) => _engine.AdvanceTime(elapsedSeconds);

    private void OnPreLoad()
    {
        _engine.Dispatch("game_loading");
        foreach (ScriptModSession session in _engine.Mods)
        {
            foreach (ItemDeclaration declaration in session.Items)
            {
                try
                {
                    S1API.Items.ItemDefinition item = _items.Register(declaration);
                    _registeredItems[declaration.Id] = item;
                }
                catch (Exception ex)
                {
                    _host.Log(S1LuaLogLevel.Error, session.DisplayName, $"Could not register item '{declaration.LocalId}': {ex.Message}");
                }
            }
        }
    }

    private void OnLoadComplete()
    {
        _ = LevelManager.Exists;
        foreach (ScriptModSession session in _engine.Mods)
        {
            foreach (ItemDeclaration declaration in session.Items)
            {
                if (!_registeredItems.TryGetValue(declaration.Id, out S1API.Items.ItemDefinition? item))
                    continue;
                try
                {
                    _items.AddToShops(declaration, item);
                }
                catch (Exception ex)
                {
                    _host.Log(S1LuaLogLevel.Error, session.DisplayName, $"Could not add '{declaration.LocalId}' to shops: {ex.Message}");
                }
            }
        }

        CreateMapMarkers();
        BindPlayerSubscriptions();
        _engine.BindRuntimeSubscriptions();
        if (Player.Local != null)
            DispatchPlayerReady();
        _engine.Dispatch("game_loaded");
    }

    private void OnPreSceneChange()
    {
        _engine.Dispatch("scene_changing");
        _playerReadyDispatched = false;
        _engine.UnbindRuntimeSubscriptions();
        DisposePlayerSubscriptions();
        DisposeMapMarkers();
        _registeredItems.Clear();
    }

    private void OnSaveStart() => _engine.Dispatch("before_save");

    private void OnSaveComplete() => _engine.Dispatch("after_save");

    private void OnHourPassed() => _engine.Dispatch("hour_passed");

    private void OnDayPassed() => _engine.Dispatch("day_passed");

    private void OnWeekPassed() => _engine.Dispatch("week_passed");

    private void OnSleepStarted() => _engine.Dispatch("sleep_started");

    private void OnSleepEnded(int minutesSkipped) => _engine.Dispatch("sleep_ended", minutesSkipped);

    private void OnWeatherChanged(WeatherState state) => _engine.Dispatch("weather_changed");

    private void OnTrashRecycled(int itemCount) => _engine.Dispatch("trash_recycled", itemCount);

    private void OnBalanceChanged()
    {
        if (_dispatchingBalanceChanged)
            return;

        try
        {
            _dispatchingBalanceChanged = true;
            _engine.Dispatch("balance_changed");
        }
        finally
        {
            _dispatchingBalanceChanged = false;
        }
    }

    private void OnXpChanged(FullRank before, FullRank after)
    {
        if (_dispatchingXpChanged)
            return;

        try
        {
            _dispatchingXpChanged = true;
            _engine.Dispatch("xp_changed");
        }
        finally
        {
            _dispatchingXpChanged = false;
        }
    }

    private void OnRankUp(FullRank before, FullRank after)
    {
        if (_dispatchingRankUp)
            return;

        try
        {
            _dispatchingRankUp = true;
            _engine.Dispatch("rank_up");
        }
        finally
        {
            _dispatchingRankUp = false;
        }
    }

    private void OnLocalPlayerSpawned(Player player)
    {
        BindPlayerSubscriptions();
        DispatchPlayerReady();
    }

    private void DispatchPlayerReady()
    {
        if (_playerReadyDispatched)
            return;
        _playerReadyDispatched = true;
        _engine.Dispatch("player_ready");
    }

    private void BindPlayerSubscriptions()
    {
        DisposePlayerSubscriptions();
        _playerDiedSubscription = _host.SubscribePlayerDied(() => _engine.Dispatch("player_died"));
        _playerRevivedSubscription = _host.SubscribePlayerRevived(() => _engine.Dispatch("player_revived"));
    }

    private void DisposePlayerSubscriptions()
    {
        _playerDiedSubscription?.Dispose();
        _playerDiedSubscription = null;
        _playerRevivedSubscription?.Dispose();
        _playerRevivedSubscription = null;
    }

    private void CreateMapMarkers()
    {
        DisposeMapMarkers();
        foreach (ScriptModSession session in _engine.Mods)
        {
            foreach (MapMarkerRequest marker in session.Markers)
            {
                try
                {
                    _mapMarkers.Add(_host.CreateMapMarker(marker));
                }
                catch (Exception ex)
                {
                    _host.Log(
                        S1LuaLogLevel.Error,
                        session.DisplayName,
                        $"Could not create map marker '{marker.Id}': {ex.Message}");
                }
            }
        }
    }

    private void DisposeMapMarkers()
    {
        foreach (IDisposable marker in _mapMarkers)
        {
            try
            {
                marker.Dispose();
            }
            catch (Exception ex)
            {
                _host.Log(S1LuaLogLevel.Warning, "Map markers", ex.Message);
            }
        }
        _mapMarkers.Clear();
    }
}
