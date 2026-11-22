using System.Globalization;
using System.Text.Json.Serialization;

namespace WeatherStation.Models;

public class WeatherLocation
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Unique key, used later for favourites and the cache.
    [JsonIgnore]
    public string Id =>
        string.Create(CultureInfo.InvariantCulture, $"{Latitude:F2}_{Longitude:F2}");

    [JsonIgnore]
    public string DisplayName => $"{Name}, {Country}";
}