using WeatherStation.Models;

namespace WeatherStation.Services;

public static class LocationCatalog
{
    public static IReadOnlyList<WeatherLocation> All { get; } = new List<WeatherLocation>
    {
        new() { Name = "Galway",    Country = "Ireland",       Latitude = 53.27,  Longitude = -9.05 },
        new() { Name = "Dublin",    Country = "Ireland",       Latitude = 53.35,  Longitude = -6.26 },
        new() { Name = "Cork",      Country = "Ireland",       Latitude = 51.90,  Longitude = -8.47 },
        new() { Name = "Limerick",  Country = "Ireland",       Latitude = 52.66,  Longitude = -8.63 },
        new() { Name = "Westport",  Country = "Ireland",       Latitude = 53.80,  Longitude = -9.52 },
        new() { Name = "Belfast",   Country = "United Kingdom", Latitude = 54.60,  Longitude = -5.93 },
        new() { Name = "London",    Country = "United Kingdom", Latitude = 51.51,  Longitude = -0.13 },
        new() { Name = "Paris",     Country = "France",        Latitude = 48.85,  Longitude = 2.35 },
        new() { Name = "Odesa",     Country = "Ukraine",       Latitude = 46.48,  Longitude = 30.72 },
        new() { Name = "New York",  Country = "USA",           Latitude = 40.71,  Longitude = -74.01 },
        new() { Name = "Tokyo",     Country = "Japan",         Latitude = 35.68,  Longitude = 139.69 },
        new() { Name = "Sydney",    Country = "Australia",     Latitude = -33.87, Longitude = 151.21 }
    };
}