using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using WeatherStation.Models;
using WeatherStation.Services;

namespace WeatherStation.ViewModels;

public class LocationsViewModel : BaseViewModel
{
    private readonly IUserDataService _userData;

    private IReadOnlyList<WeatherLocation> _favourites = [];
    private IReadOnlyList<WeatherLocation> _recent = [];
    private string _searchText = string.Empty;
    private string? _statusMessage;
    private bool _isNavigating;

    public LocationsViewModel(IUserDataService userData)
    {
        _userData = userData;

        SelectCommand = new Command<WeatherLocation>(async location => await SelectAsync(location));
        ToggleFavouriteCommand = new Command<WeatherLocation>(async location => await ToggleFavouriteAsync(location));

        // Show the full catalogue straight away; favourites and recent arrive in LoadAsync.
        Rebuild();
    }

    // ---- State the page binds to ----

    public ObservableCollection<LocationItem> FavouriteItems { get; } = new();
    public ObservableCollection<LocationItem> RecentItems { get; } = new();
    public ObservableCollection<LocationItem> AllItems { get; } = new();

    public ICommand SelectCommand { get; }
    public ICommand ToggleFavouriteCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
            {
                Rebuild();
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
            }
        }
    }

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);
    public bool HasFavourites => FavouriteItems.Count > 0;
    public bool HasRecent => RecentItems.Count > 0;
    public bool ShowNoResults => AllItems.Count == 0;

    // Only hint when the user has no favourites at all (not just none matching the search).
    public bool ShowFavouritesHint =>
        _favourites.Count == 0 && string.IsNullOrWhiteSpace(_searchText);

    // ---- Actions ----

    // Called each time the page appears so Recent reflects forecasts viewed since last time.
    public async Task LoadAsync()
    {
        try
        {
            _favourites = await _userData.GetFavouritesAsync();
            _recent = await _userData.GetRecentAsync();
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not load saved locations: {ex}");
            StatusMessage = "Your saved locations could not be loaded.";
        }

        Rebuild();
    }

    private async Task SelectAsync(WeatherLocation? location)
    {
        if (location is null || _isNavigating || Shell.Current is null)
        {
            return;
        }

        _isNavigating = true;
        try
        {
            // Passed once; ForecastViewModel picks it up in ApplyQueryAttributes.
            var parameters = new ShellNavigationQueryParameters
            {
                { "location", location }
            };

            await Shell.Current.GoToAsync("//ForecastPage", parameters);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation to forecast failed: {ex}");
            StatusMessage = "Could not open that location.";
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async Task ToggleFavouriteAsync(WeatherLocation? location)
    {
        if (location is null)
        {
            return;
        }

        try
        {
            await _userData.ToggleFavouriteAsync(location);
            _favourites = await _userData.GetFavouritesAsync();
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not update favourites: {ex}");
            StatusMessage = "Your favourites could not be saved.";
        }

        Rebuild();
    }

    // ---- Building the lists ----

    private void Rebuild()
    {
        var favouriteIds = _favourites.Select(f => f.Id).ToHashSet();

        Fill(FavouriteItems, _favourites, favouriteIds);
        Fill(RecentItems, _recent, favouriteIds);
        Fill(AllItems, LocationCatalog.All, favouriteIds);

        OnPropertyChanged(nameof(HasFavourites));
        OnPropertyChanged(nameof(HasRecent));
        OnPropertyChanged(nameof(ShowNoResults));
        OnPropertyChanged(nameof(ShowFavouritesHint));
    }

    private void Fill(
        ObservableCollection<LocationItem> target,
        IEnumerable<WeatherLocation> source,
        HashSet<string> favouriteIds)
    {
        target.Clear();
        foreach (var location in source.Where(Matches))
        {
            target.Add(new LocationItem(
                location,
                favouriteIds.Contains(location.Id),
                SelectCommand,
                ToggleFavouriteCommand));
        }
    }

    private bool Matches(WeatherLocation location)
    {
        var text = _searchText.Trim();
        if (text.Length == 0)
        {
            return true;
        }

        return location.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase)
               || location.Country.Contains(text, StringComparison.CurrentCultureIgnoreCase);
    }
}