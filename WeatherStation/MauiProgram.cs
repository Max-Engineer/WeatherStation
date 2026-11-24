using Microsoft.Extensions.Logging;
using WeatherStation.Services;
using WeatherStation.Views;
using WeatherStation.ViewModels;

namespace WeatherStation
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            // Services
            builder.Services.AddSingleton(new HttpClient { Timeout = TimeSpan.FromSeconds(15) });
            builder.Services.AddSingleton<IWeatherService, WeatherService>();
            builder.Services.AddSingleton<IJsonStorageService>(_ => new JsonStorageService(FileSystem.AppDataDirectory));
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<IUserDataService, UserDataService>();
            builder.Services.AddSingleton<IForecastCache, ForecastCache>();
            builder.Services.AddSingleton<IForecastRepository, ForecastRepository>();

            // Pages (registered so they can receive services in their constructors)
            builder.Services.AddTransient<ForecastPage>();
            builder.Services.AddTransient<ForecastViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
