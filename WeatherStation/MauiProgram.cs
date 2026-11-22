using Microsoft.Extensions.Logging;
using WeatherStation.Services;
using WeatherStation.Views;

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

            // Pages (registered so they can receive services in their constructors)
            builder.Services.AddTransient<ForecastPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
