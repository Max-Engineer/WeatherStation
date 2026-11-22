namespace WeatherStation.Models;

public class ForecastData
{
    public WeatherLocation Location { get; set; } = new();

    // UTC time the forecast was downloaded. Required by the brief for caching.
    public DateTime RetrievedAt { get; set; }

    public CurrentWeather? Current { get; set; }
    public List<DailyForecast> Days { get; set; } = new();
}

public class CurrentWeather
{
    public DateTime Time { get; set; }
    public double? Temperature { get; set; }
    public double? ApparentTemperature { get; set; }
    public int? Humidity { get; set; }
    public double? Precipitation { get; set; }

    // WMO weather code. -1 means unknown or missing.
    public int WeatherCode { get; set; } = -1;
    public double? WindSpeed { get; set; }
}

public class DailyForecast
{
    public DateOnly Date { get; set; }
    public int WeatherCode { get; set; } = -1;
    public double? TempMax { get; set; }
    public double? TempMin { get; set; }
    public int? PrecipitationChance { get; set; }
    public double? WindSpeedMax { get; set; }
}