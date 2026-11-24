using WeatherStation.Models;

namespace WeatherStation.Services;

public interface IForecastRepository
{
    Task<ForecastResult> GetForecastAsync(
        WeatherLocation location,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);
}

public class ForecastRepository : IForecastRepository
{
    private readonly IWeatherService _weatherService;
    private readonly IForecastCache _cache;
    private readonly ISettingsService _settingsService;

    public ForecastRepository(
        IWeatherService weatherService,
        IForecastCache cache,
        ISettingsService settingsService)
    {
        _weatherService = weatherService;
        _cache = cache;
        _settingsService = settingsService;
    }

    public async Task<ForecastResult> GetForecastAsync(
        WeatherLocation location,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsService.GetAsync();
        var cached = await _cache.GetAsync(location.Id);

        // 1. A recent saved copy: use it and don't touch the API.
        if (cached is not null && !forceRefresh && IsFresh(cached, settings))
        {
            return ForecastResult.Cached(cached);
        }

        // 2. Otherwise ask the API and remember the answer.
        var live = await _weatherService.GetForecastAsync(location, cancellationToken);
        if (live.IsSuccess)
        {
            await _cache.SaveAsync(live.Data);
            return ForecastResult.Live(live.Data);
        }

        // 3. API failed but we have an older copy: show it with a warning.
        if (cached is not null)
        {
            var saved = cached.RetrievedAt.ToLocalTime();
            return ForecastResult.Cached(
                cached,
                $"{live.ErrorMessage} Showing the forecast saved on {saved:dd MMM} at {saved:HH:mm}.");
        }

        // 4. Nothing to show.
        return ForecastResult.Failed(live.ErrorMessage ?? "The forecast could not be loaded.");
    }

    private static bool IsFresh(ForecastData data, AppSettings settings)
    {
        var age = DateTime.UtcNow - data.RetrievedAt;

        // age >= 0 stops a clock change from making an old copy look fresh forever.
        return age >= TimeSpan.Zero && age < TimeSpan.FromMinutes(settings.RefreshIntervalMinutes);
    }
}