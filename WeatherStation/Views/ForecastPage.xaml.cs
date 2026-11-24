using System.Diagnostics;
using WeatherStation.ViewModels;

namespace WeatherStation.Views;

public partial class ForecastPage : ContentPage
{
    private readonly ForecastViewModel _viewModel;

    public ForecastPage(ForecastViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private async void OnCreditTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri("https://open-meteo.com/"));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not open link: {ex.Message}");
        }
    }
}