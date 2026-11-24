using WeatherStation.Models;

namespace WeatherStation.Services;

public interface IUserDataService
{
    Task<IReadOnlyList<WeatherLocation>> GetFavouritesAsync();
    Task<IReadOnlyList<WeatherLocation>> GetRecentAsync();
    Task<WeatherLocation?> GetLastSelectedAsync();
    Task<bool> IsFavouriteAsync(WeatherLocation location);

    // Returns true if the location is now a favourite, false if it was removed.
    Task<bool> ToggleFavouriteAsync(WeatherLocation location);

    // Marks a location as last selected and adds it to the recent list.
    Task RecordViewedAsync(WeatherLocation location);
}

public class UserDataService : IUserDataService
{
    private const string FileName = "userdata.json";
    private const int MaxRecent = 5;

    private readonly IJsonStorageService _storage;
    private Task<UserData>? _loadTask;

    public UserDataService(IJsonStorageService storage)
    {
        _storage = storage;
    }

    public async Task<IReadOnlyList<WeatherLocation>> GetFavouritesAsync()
    {
        var data = await GetDataAsync();
        return data.Favourites.ToList();
    }

    public async Task<IReadOnlyList<WeatherLocation>> GetRecentAsync()
    {
        var data = await GetDataAsync();
        return data.Recent.ToList();
    }

    public async Task<WeatherLocation?> GetLastSelectedAsync()
    {
        var data = await GetDataAsync();
        return data.LastSelected;
    }

    public async Task<bool> IsFavouriteAsync(WeatherLocation location)
    {
        var data = await GetDataAsync();
        return data.Favourites.Any(f => f.Id == location.Id);
    }

    public async Task<bool> ToggleFavouriteAsync(WeatherLocation location)
    {
        var data = await GetDataAsync();
        var existing = data.Favourites.FirstOrDefault(f => f.Id == location.Id);

        if (existing is not null)
        {
            data.Favourites.Remove(existing);
        }
        else
        {
            data.Favourites.Add(location);
        }

        await SaveAsync(data);
        return existing is null;
    }

    public async Task RecordViewedAsync(WeatherLocation location)
    {
        var data = await GetDataAsync();

        data.LastSelected = location;
        data.Recent.RemoveAll(l => l.Id == location.Id);
        data.Recent.Insert(0, location);

        if (data.Recent.Count > MaxRecent)
        {
            data.Recent.RemoveRange(MaxRecent, data.Recent.Count - MaxRecent);
        }

        await SaveAsync(data);
    }

    private Task<UserData> GetDataAsync() => _loadTask ??= LoadAsync();

    private async Task<UserData> LoadAsync()
    {
        var data = await _storage.LoadAsync<UserData>(FileName);

        // Repair anything a damaged or hand-edited file might have left behind.
        data.Favourites ??= new();
        data.Recent ??= new();
        data.Favourites.RemoveAll(l => l is null);
        data.Recent.RemoveAll(l => l is null);

        return data;
    }

    private Task SaveAsync(UserData data) => _storage.SaveAsync(FileName, data);
}