namespace WeatherStation.Models;

public enum TemperatureUnit
{
    Celsius,
    Fahrenheit
}

public enum WindSpeedUnit
{
    KilometresPerHour,
    MilesPerHour,
    MetresPerSecond
}

public enum ThemeChoice
{
    System,
    Light,
    Dark
}

public class AppSettings
{
    public TemperatureUnit TemperatureUnit { get; set; } = TemperatureUnit.Celsius;
    public WindSpeedUnit WindSpeedUnit { get; set; } = WindSpeedUnit.KilometresPerHour;
    public ThemeChoice Theme { get; set; } = ThemeChoice.System;

    // Id of the location to open on start-up (null = last selected).
    public string? DefaultLocationId { get; set; }

    // How long a saved forecast counts as fresh before we call the API again.
    public int RefreshIntervalMinutes { get; set; } = 30;
}