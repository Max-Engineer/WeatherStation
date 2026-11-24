using WeatherStation.Models;

namespace WeatherStation.Helpers;

// Input values are always metric (°C, km/h, mm). These methods convert and format for display.
public static class WeatherFormatter
{
    public static string Temperature(double? celsius, TemperatureUnit unit)
    {
        if (celsius is null)
        {
            return "--";
        }

        var value = unit == TemperatureUnit.Fahrenheit
            ? celsius.Value * 9 / 5 + 32
            : celsius.Value;

        // Rounding to an int avoids "-0°".
        var rounded = (int)Math.Round(value, MidpointRounding.AwayFromZero);
        return $"{rounded}°";
    }

    public static string Wind(double? kmh, WindSpeedUnit unit)
    {
        if (kmh is null)
        {
            return "--";
        }

        return unit switch
        {
            WindSpeedUnit.MilesPerHour => $"{(int)Math.Round(kmh.Value * 0.621371)} mph",
            WindSpeedUnit.MetresPerSecond => $"{kmh.Value / 3.6:0.#} m/s",
            _ => $"{(int)Math.Round(kmh.Value)} km/h"
        };
    }

    public static string Percent(int? value) => value is null ? "--" : $"{value}%";

    public static string Millimetres(double? value) => value is null ? "--" : $"{value:0.#} mm";
}