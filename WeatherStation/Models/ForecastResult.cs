using System.Diagnostics.CodeAnalysis;

namespace WeatherStation.Models;

public class ForecastResult
{
    public ForecastData? Data { get; private set; }
    public bool IsFromCache { get; private set; }

    // A warning when showing saved data after a failed refresh, or the error when there is no data.
    public string? Message { get; private set; }

    [MemberNotNullWhen(true, nameof(Data))]
    public bool HasData => Data is not null;

    public static ForecastResult Live(ForecastData data) =>
        new() { Data = data };

    public static ForecastResult Cached(ForecastData data, string? warning = null) =>
        new() { Data = data, IsFromCache = true, Message = warning };

    public static ForecastResult Failed(string message) =>
        new() { Message = message };
}