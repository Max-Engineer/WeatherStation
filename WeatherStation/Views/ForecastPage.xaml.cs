using System.Text;
using WeatherStation.Models;
using WeatherStation.Services;

namespace WeatherStation.Views;

public partial class ForecastPage : ContentPage
{
    private readonly IWeatherService _weatherService;

    public ForecastPage(IWeatherService weatherService)
    {
        InitializeComponent();
        _weatherService = weatherService;

        LocationPicker.ItemsSource = LocationCatalog.All.ToList();
        LocationPicker.ItemDisplayBinding = new Binding(nameof(WeatherLocation.DisplayName));
        LocationPicker.SelectedIndex = 0;
    }

    private async void OnFetchClicked(object? sender, EventArgs e)
    {
        if (LocationPicker.SelectedItem is not WeatherLocation location)
        {
            ResultLabel.Text = "Pick a location first.";
            return;
        }

        Spinner.IsRunning = true;
        ResultLabel.Text = string.Empty;

        var result = await _weatherService.GetForecastAsync(location);

        Spinner.IsRunning = false;

        if (!result.IsSuccess)
        {
            ResultLabel.Text = result.ErrorMessage;
            return;
        }

        var data = result.Data;
        var sb = new StringBuilder();
        sb.AppendLine(data.Location.DisplayName);
        sb.AppendLine($"Retrieved (UTC): {data.RetrievedAt:HH:mm:ss}");
        sb.AppendLine($"Now: {data.Current?.Temperature:F1} °C, wind {data.Current?.WindSpeed:F0} km/h, humidity {data.Current?.Humidity}%, code {data.Current?.WeatherCode}");
        sb.AppendLine();

        foreach (var day in data.Days)
        {
            sb.AppendLine($"{day.Date:ddd dd MMM}: {day.TempMin:F0}° to {day.TempMax:F0}°, rain {day.PrecipitationChance}%, wind {day.WindSpeedMax:F0} km/h, code {day.WeatherCode}");
        }

        ResultLabel.Text = sb.ToString();
    }
}