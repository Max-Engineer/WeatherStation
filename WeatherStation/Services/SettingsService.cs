using WeatherStation.Models;

namespace WeatherStation.Services;

public interface ISettingsService
{
    event EventHandler? SettingsChanged;

    // Loads once, then returns the same object every time.
    Task<AppSettings> GetAsync();

    // Saves the current settings object and raises SettingsChanged.
    Task SaveAsync();
}

public class SettingsService : ISettingsService
{
    private const string FileName = "settings.json";
    private readonly IJsonStorageService _storage;
    private Task<AppSettings>? _loadTask;

    public event EventHandler? SettingsChanged;

    public SettingsService(IJsonStorageService storage)
    {
        _storage = storage;
    }

    public Task<AppSettings> GetAsync() => _loadTask ??= LoadAsync();

    public async Task SaveAsync()
    {
        var settings = await GetAsync();
        await _storage.SaveAsync(FileName, settings);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task<AppSettings> LoadAsync()
    {
        var settings = await _storage.LoadAsync<AppSettings>(FileName);

        // Guard against a hand-edited or nonsense value.
        if (settings.RefreshIntervalMinutes < 5)
        {
            settings.RefreshIntervalMinutes = 30;
        }

        return settings;
    }
}