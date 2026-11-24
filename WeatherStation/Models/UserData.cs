namespace WeatherStation.Models;

public class UserData
{
    public List<WeatherLocation> Favourites { get; set; } = new();
    public List<WeatherLocation> Recent { get; set; } = new();
    public WeatherLocation? LastSelected { get; set; }
}