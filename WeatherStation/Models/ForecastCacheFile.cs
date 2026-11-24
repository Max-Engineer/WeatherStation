namespace WeatherStation.Models;

public class ForecastCacheFile
{
    // Key = WeatherLocation.Id
    public Dictionary<string, ForecastData> Entries { get; set; } = new();
}