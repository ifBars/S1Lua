using S1API.GameTime;
using S1API.Weather;
using S1Lua.Hosting;

namespace S1Lua.Runtime;

internal sealed class S1ApiWorldService
{
    internal GameTimeSnapshot? GetTime()
    {
        try
        {
            return new GameTimeSnapshot(
                TimeManager.CurrentDay.ToString().ToLowerInvariant(),
                TimeManager.CurrentTime,
                TimeManager.GetFormatted12HourTime(),
                TimeManager.ElapsedDays,
                TimeManager.IsNight,
                TimeManager.SleepInProgress);
        }
        catch
        {
            return null;
        }
    }

    internal WeatherSnapshot? GetWeather()
    {
        WeatherState? state = WeatherManager.Current;
        return state.HasValue
            ? new WeatherSnapshot(
                state.Value.Sunny,
                state.Value.Cloudy,
                state.Value.Rainy,
                state.Value.Stormy,
                state.Value.Snowy,
                state.Value.Foggy,
                state.Value.Windy,
                state.Value.Hail,
                state.Value.Sleet)
            : null;
    }
}
