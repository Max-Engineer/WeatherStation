namespace WeatherStation.Helpers;

// Icon is the image file name in Resources/Images (SVG files are referenced with a .png extension).
public record WeatherCodeInfo(string Description, string Icon);

public static class WeatherCodes
{
    // WMO weather codes as used by Open-Meteo.
    public static WeatherCodeInfo Describe(int code) => code switch
    {
        0 => new("Clear sky", "weather_clear.png"),
        1 => new("Mainly clear", "weather_clear.png"),
        2 => new("Partly cloudy", "weather_partly_cloudy.png"),
        3 => new("Overcast", "weather_cloudy.png"),
        45 or 48 => new("Fog", "weather_fog.png"),
        51 or 53 or 55 => new("Drizzle", "weather_drizzle.png"),
        56 or 57 => new("Freezing drizzle", "weather_drizzle.png"),
        61 => new("Light rain", "weather_rain.png"),
        63 => new("Moderate rain", "weather_rain.png"),
        65 => new("Heavy rain", "weather_rain.png"),
        66 or 67 => new("Freezing rain", "weather_rain.png"),
        71 or 73 or 75 => new("Snow", "weather_snow.png"),
        77 => new("Snow grains", "weather_snow.png"),
        80 or 81 => new("Rain showers", "weather_showers.png"),
        82 => new("Violent rain showers", "weather_showers.png"),
        85 or 86 => new("Snow showers", "weather_snow.png"),
        95 => new("Thunderstorm", "weather_thunder.png"),
        96 or 99 => new("Thunderstorm with hail", "weather_thunder.png"),
        _ => new("Unknown conditions", "weather_unknown.png")
    };
}