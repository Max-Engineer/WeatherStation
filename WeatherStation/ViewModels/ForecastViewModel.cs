using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using WeatherStation.Helpers;
using WeatherStation.Models;
using WeatherStation.Services;

namespace WeatherStation.ViewModels;

public class ForecastViewModel : BaseViewModel
{
    private readonly IForecastRepository _repository;
    private readonly IUserDataService _userData;
    private readonly ISettingsService _settingsService;

    private AppSettings _settings = new();
    private ForecastData? _forecast;
    private bool _isFromCache;
    private WeatherLocation? _selectedLocation;
    private bool _isBusy;
    private string? _statusMessage;
    private bool _initialized;
    private CancellationTokenSource? _loadCts;

    public ForecastViewModel(
        IForecastRepository repository,
        IUserDataService userData,
        ISettingsService settingsService)
    {
        _repository = repository;
        _userData = userData;
        _settingsService = settingsService;

        RefreshCommand = new Command(async () => await LoadForecastAsync(forceRefresh: true));

        // When settings change (Part 9), redraw using the new units.
        _settingsService.SettingsChanged += (_, _) => RedisplayForecast();
    }

    // ---- State the page binds to ----

    public IReadOnlyList<WeatherLocation> Locations => LocationCatalog.All;

    public ObservableCollection<DailyForecastItem> Days { get; } = new();

    public ICommand RefreshCommand { get; }

    public WeatherLocation? SelectedLocation
    {
        get => _selectedLocation;
        set
        {
            if (!SetProperty(ref _selectedLocation, value))
            {
                return;
            }

            if (_initialized && value is not null)
            {
                // LoadForecastAsync handles its own errors, so fire-and-forget is safe here.
                _ = LoadForecastAsync(forceRefresh: false);
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(ShowEmptyState));
            }
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatus));
                OnPropertyChanged(nameof(ShowEmptyState));
            }
        }
    }

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);
    public bool HasData => _forecast is not null;
    public bool ShowEmptyState => !HasData && !IsBusy && !HasStatus;

    // ---- Current weather, formatted using the user's units ----

    public string LocationName =>
        _forecast?.Location.DisplayName ?? SelectedLocation?.DisplayName ?? string.Empty;

    public string TemperatureText =>
        WeatherFormatter.Temperature(_forecast?.Current?.Temperature, _settings.TemperatureUnit);

    public string FeelsLikeText =>
        $"Feels like {WeatherFormatter.Temperature(_forecast?.Current?.ApparentTemperature, _settings.TemperatureUnit)}";

    public string ConditionText =>
        WeatherCodes.Describe(_forecast?.Current?.WeatherCode ?? -1).Description;

    public string ConditionIcon =>
    WeatherCodes.Describe(_forecast?.Current?.WeatherCode ?? -1).Icon;

    public string WindText =>
        WeatherFormatter.Wind(_forecast?.Current?.WindSpeed, _settings.WindSpeedUnit);

    public string HumidityText =>
        WeatherFormatter.Percent(_forecast?.Current?.Humidity);

    public string RainNowText =>
        WeatherFormatter.Millimetres(_forecast?.Current?.Precipitation);

    public string UpdatedText
    {
        get
        {
            if (_forecast is null)
            {
                return string.Empty;
            }

            var time = _forecast.RetrievedAt.ToLocalTime();
            return _isFromCache
                ? $"Saved copy from {time:dd MMM HH:mm}"
                : $"Updated {time:HH:mm}";
        }
    }

    // ---- Actions ----

    // Called by the page each time it appears; only the first call does any work.
    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _settings = await _settingsService.GetAsync();

        // Start with the default location, else the last one viewed, else the first in the list.
        // We match by Id so we pick the same object the Picker is showing.
        var last = await _userData.GetLastSelectedAsync();
        SelectedLocation = FindInCatalog(_settings.DefaultLocationId)
                           ?? FindInCatalog(last?.Id)
                           ?? LocationCatalog.All[0];
    }

    private async Task LoadForecastAsync(bool forceRefresh)
    {
        var location = SelectedLocation;
        if (location is null)
        {
            return;
        }

        // A newer request replaces any request still running.
        _loadCts?.Cancel();
        var cts = new CancellationTokenSource();
        _loadCts = cts;

        IsBusy = true;
        StatusMessage = null;

        ForecastResult result;
        try
        {
            result = await _repository.GetForecastAsync(location, forceRefresh, cts.Token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error loading forecast: {ex}");
            result = ForecastResult.Failed("Something went wrong while loading the weather.");
        }

        if (cts.IsCancellationRequested)
        {
            return; // the newer request owns the screen now
        }

        IsBusy = false;

        if (result.HasData)
        {
            ApplyForecast(result.Data, result.IsFromCache);
            StatusMessage = result.Message; // a warning if this is a saved copy after a failed refresh
            await _userData.RecordViewedAsync(location);
        }
        else
        {
            ClearForecast(); // never leave another city's weather on screen
            StatusMessage = result.Message;
        }
    }

    private void ApplyForecast(ForecastData forecast, bool fromCache)
    {
        _forecast = forecast;
        _isFromCache = fromCache;

        // "Today" means today in the forecast location, not on this device.
        DateOnly? locationToday = forecast.Current is null
            ? null
            : DateOnly.FromDateTime(forecast.Current.Time);

        Days.Clear();
        foreach (var day in forecast.Days)
        {
            Days.Add(BuildDayItem(day, locationToday));
        }

        OnPropertyChanged(string.Empty);
    }

    private void ClearForecast()
    {
        _forecast = null;
        Days.Clear();
        OnPropertyChanged(string.Empty);
    }

    private void RedisplayForecast()
    {
        if (_forecast is not null)
        {
            ApplyForecast(_forecast, _isFromCache);
        }
    }

    private DailyForecastItem BuildDayItem(DailyForecast day, DateOnly? locationToday)
    {
        var info = WeatherCodes.Describe(day.WeatherCode);
        var rain = WeatherFormatter.Percent(day.PrecipitationChance);
        var wind = WeatherFormatter.Wind(day.WindSpeedMax, _settings.WindSpeedUnit);

        return new DailyForecastItem
        {
            DayLabel = day.Date == locationToday ? "Today" : day.Date.ToString("ddd"),
            DateLabel = day.Date.ToString("dd MMM"),
            Icon = info.Icon,
            Description = info.Description,
            DetailText = $"Rain {rain} · Wind {wind}",
            HighText = WeatherFormatter.Temperature(day.TempMax, _settings.TemperatureUnit),
            LowText = WeatherFormatter.Temperature(day.TempMin, _settings.TemperatureUnit)
        };
    }

    private static WeatherLocation? FindInCatalog(string? id) =>
        id is null ? null : LocationCatalog.All.FirstOrDefault(l => l.Id == id);
}