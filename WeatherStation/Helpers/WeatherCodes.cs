namespace WeatherStation.Helpers;

public record WeatherCodeInfo(string Description, string Emoji);

public static class WeatherCodes
{
    // WMO weather codes as used by Open-Meteo. The emoji are temporary: Part 5 replaces them with SVG icons.
    public static WeatherCodeInfo Describe(int code) => code switch
    {
        0 => new("Clear sky", "☀️"),
        1 => new("Mainly clear", "🌤️"),
        2 => new("Partly cloudy", "⛅"),
        3 => new("Overcast", "☁️"),
        45 or 48 => new("Fog", "🌫️"),
        51 or 53 or 55 => new("Drizzle", "🌦️"),
        56 or 57 => new("Freezing drizzle", "🌧️"),
        61 => new("Light rain", "🌧️"),
        63 => new("Moderate rain", "🌧️"),
        65 => new("Heavy rain", "🌧️"),
        66 or 67 => new("Freezing rain", "🌧️"),
        71 or 73 or 75 => new("Snow", "🌨️"),
        77 => new("Snow grains", "🌨️"),
        80 or 81 => new("Rain showers", "🌦️"),
        82 => new("Violent rain showers", "🌧️"),
        85 or 86 => new("Snow showers", "🌨️"),
        95 => new("Thunderstorm", "⛈️"),
        96 or 99 => new("Thunderstorm with hail", "⛈️"),
        _ => new("Unknown conditions", "❓")
    };
}