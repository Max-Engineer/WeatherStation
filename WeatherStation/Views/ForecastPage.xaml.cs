using System.Text;
using WeatherStation.Models;
using WeatherStation.Services;

namespace WeatherStation.Views;

public partial class ForecastPage : ContentPage
{
    private readonly IForecastRepository _repository;
    private readonly IUserDataService _userData;
    private readonly ISettingsService _settings;

    public ForecastPage(
        IForecastRepository repository,
        IUserDataService userData,
        ISettingsService settings)
    {
        InitializeComponent();
        _repository = repository;
        _userData = userData;
        _settings = settings;

        PathLabel.Text = $"Data folder: {FileSystem.AppDataDirectory}";
        LocationPicker.ItemsSource = LocationCatalog.All.ToList();
        LocationPicker.ItemDisplayBinding = new Binding(nameof(WeatherLocation.DisplayName));
        LocationPicker.SelectedIndex = 0;
    }

    private WeatherLocation? SelectedLocation => LocationPicker.SelectedItem as WeatherLocation;

    private async void OnFetchClicked(object? sender, EventArgs e) => await LoadForecastAsync(false);

    private async void OnRefreshClicked(object? sender, EventArgs e) => await LoadForecastAsync(true);

    private async Task LoadForecastAsync(bool forceRefresh)
    {
        var location = SelectedLocation;
        if (location is null)
        {
            ResultLabel.Text = "Pick a location first.";
            return;
        }

        Spinner.IsRunning = true;
        var result = await _repository.GetForecastAsync(location, forceRefresh);
        Spinner.IsRunning = false;

        if (!result.HasData)
        {
            ResultLabel.Text = result.Message;
            return;
        }

        await _userData.RecordViewedAsync(location);

        var data = result.Data;
        var sb = new StringBuilder();
        sb.AppendLine(data.Location.DisplayName);
        sb.AppendLine(result.IsFromCache
            ? $"Source: saved copy from {data.RetrievedAt.ToLocalTime():HH:mm:ss}"
            : "Source: live from Open-Meteo");

        if (result.Message is not null)
        {
            sb.AppendLine($"Warning: {result.Message}");
        }

        sb.AppendLine($"Now: {data.Current?.Temperature:F1} °C, wind {data.Current?.WindSpeed:F0} km/h");
        sb.AppendLine();

        foreach (var day in data.Days)
        {
            sb.AppendLine($"{day.Date:ddd dd MMM}: {day.TempMin:F0}° to {day.TempMax:F0}°, rain {day.PrecipitationChance}%");
        }

        ResultLabel.Text = sb.ToString();
    }

    private async void OnFavouriteClicked(object? sender, EventArgs e)
    {
        var location = SelectedLocation;
        if (location is null)
        {
            return;
        }

        var isFavourite = await _userData.ToggleFavouriteAsync(location);
        ResultLabel.Text = $"{location.DisplayName} is {(isFavourite ? "now" : "no longer")} a favourite.";
    }

    private async void OnToggleUnitClicked(object? sender, EventArgs e)
    {
        var settings = await _settings.GetAsync();
        settings.TemperatureUnit = settings.TemperatureUnit == TemperatureUnit.Celsius
            ? TemperatureUnit.Fahrenheit
            : TemperatureUnit.Celsius;

        await _settings.SaveAsync();
        ResultLabel.Text = $"Temperature unit saved as {settings.TemperatureUnit}.";
    }

    private async void OnShowSavedClicked(object? sender, EventArgs e)
    {
        var settings = await _settings.GetAsync();
        var favourites = await _userData.GetFavouritesAsync();
        var recent = await _userData.GetRecentAsync();
        var last = await _userData.GetLastSelectedAsync();

        var sb = new StringBuilder();
        sb.AppendLine($"Temperature unit: {settings.TemperatureUnit}");
        sb.AppendLine($"Refresh interval: {settings.RefreshIntervalMinutes} min");
        sb.AppendLine($"Favourites: {(favourites.Count == 0 ? "none" : string.Join(", ", favourites.Select(f => f.Name)))}");
        sb.AppendLine($"Recent: {(recent.Count == 0 ? "none" : string.Join(", ", recent.Select(r => r.Name)))}");
        sb.AppendLine($"Last selected: {last?.DisplayName ?? "none"}");

        ResultLabel.Text = sb.ToString();
    }
}