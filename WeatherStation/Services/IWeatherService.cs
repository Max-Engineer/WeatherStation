using WeatherStation.Models;

namespace WeatherStation.Services;

public interface IWeatherService
{
    Task<ApiResult<ForecastData>> GetForecastAsync(
        WeatherLocation location,
        CancellationToken cancellationToken = default);
}