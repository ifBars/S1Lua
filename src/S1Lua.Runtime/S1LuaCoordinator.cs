using S1API.Lifecycle;
using S1API.GameTime;
using S1API.Weather;
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
    private bool _attached;

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
        _engine.UnbindRuntimeSubscriptions();
        DisposeMapMarkers();
        _registeredItems.Clear();
    }

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
        _engine.BindRuntimeSubscriptions();
        _engine.Dispatch("game_loaded");
    }

    private void OnPreSceneChange()
    {
        _engine.Dispatch("scene_changing");
        _engine.UnbindRuntimeSubscriptions();
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
