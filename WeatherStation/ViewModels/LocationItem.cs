using System.Windows.Input;
using WeatherStation.Models;

namespace WeatherStation.ViewModels;

public class LocationItem
{
    public LocationItem(
        WeatherLocation location,
        bool isFavourite,
        ICommand selectCommand,
        ICommand toggleFavouriteCommand)
    {
        Location = location;
        IsFavourite = isFavourite;
        SelectCommand = selectCommand;
        ToggleFavouriteCommand = toggleFavouriteCommand;
    }

    public WeatherLocation Location { get; }
    public bool IsFavourite { get; }

    public string Name => Location.Name;
    public string Country => Location.Country;

    public string FavouriteGlyph => IsFavourite ? "★" : "☆";

    public string FavouriteDescription => IsFavourite
        ? $"Remove {Location.Name} from favourites"
        : $"Add {Location.Name} to favourites";

    public ICommand SelectCommand { get; }
    public ICommand ToggleFavouriteCommand { get; }
}