namespace WeatherStation.ViewModels;

// One ready-to-display row of the 7-day list.
public class DailyForecastItem
{
    public string DayLabel { get; init; } = string.Empty;
    public string DateLabel { get; init; } = string.Empty;
    public string Emoji { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DetailText { get; init; } = string.Empty;
    public string HighText { get; init; } = string.Empty;
    public string LowText { get; init; } = string.Empty;
}