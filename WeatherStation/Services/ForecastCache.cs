using WeatherStation.Models;

namespace WeatherStation.Services;

public interface IForecastCache
{
    Task<ForecastData?> GetAsync(string locationId);
    Task SaveAsync(ForecastData forecast);
}

public class ForecastCache : IForecastCache
{
    private const string FileName = "forecast_cache.json";

    private readonly IJsonStorageService _storage;
    private Task<ForecastCacheFile>? _loadTask;

    public ForecastCache(IJsonStorageService storage)
    {
        _storage = storage;
    }

    public async Task<ForecastData?> GetAsync(string locationId)
    {
        var file = await GetFileAsync();
        return file.Entries.TryGetValue(locationId, out var data) ? data : null;
    }

    public async Task SaveAsync(ForecastData forecast)
    {
        var file = await GetFileAsync();
        file.Entries[forecast.Location.Id] = forecast;
        await _storage.SaveAsync(FileName, file);
    }

    private Task<ForecastCacheFile> GetFileAsync() => _loadTask ??= LoadAsync();

    private async Task<ForecastCacheFile> LoadAsync()
    {
        var file = await _storage.LoadAsync<ForecastCacheFile>(FileName);
        file.Entries ??= new();
        return file;
    }
}