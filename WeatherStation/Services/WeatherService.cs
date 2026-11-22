using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WeatherStation.Models;

namespace WeatherStation.Services;

public class WeatherService : IWeatherService
{
    private const string BaseUrl = "https://api.open-meteo.com/v1/forecast";
    private readonly HttpClient _httpClient;

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResult<ForecastData>> GetForecastAsync(
        WeatherLocation location,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(BuildUrl(location), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<ForecastData>.Failure(DescribeStatus(response.StatusCode));
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var dto = await JsonSerializer.DeserializeAsync<OpenMeteoResponse>(
                stream, cancellationToken: cancellationToken);

            if (dto?.Current is null || dto.Daily?.Time is null)
            {
                return ApiResult<ForecastData>.Failure("The weather service returned incomplete data.");
            }

            return ApiResult<ForecastData>.Success(Map(location, dto));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ApiResult<ForecastData>.Failure("The request was cancelled.");
        }
        catch (TaskCanceledException)
        {
            // HttpClient reports a timeout this way.
            return ApiResult<ForecastData>.Failure("The weather service took too long to respond. Please try again.");
        }
        catch (HttpRequestException)
        {
            return ApiResult<ForecastData>.Failure("Could not reach the weather service. Check your internet connection.");
        }
        catch (JsonException)
        {
            return ApiResult<ForecastData>.Failure("The weather service sent a response the app could not read.");
        }
        catch (Exception ex)
        {
            // Last safety net so a surprise never crashes the app.
            Debug.WriteLine($"Unexpected weather error: {ex}");
            return ApiResult<ForecastData>.Failure("Something went wrong while loading the weather.");
        }
    }

    private static string BuildUrl(WeatherLocation location)
    {
        // Invariant culture: always a dot as the decimal separator, whatever the PC's region.
        var lat = location.Latitude.ToString(CultureInfo.InvariantCulture);
        var lon = location.Longitude.ToString(CultureInfo.InvariantCulture);

        return $"{BaseUrl}?latitude={lat}&longitude={lon}" +
               "&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,weather_code,wind_speed_10m" +
               "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,wind_speed_10m_max" +
               "&forecast_days=7&timezone=auto";
    }

    private static string DescribeStatus(HttpStatusCode status)
    {
        if (status == HttpStatusCode.TooManyRequests)
            return "Too many requests. Please wait a minute and try again.";

        if ((int)status >= 500)
            return "The weather service is temporarily unavailable. Please try again later.";

        return $"The weather service returned an error ({(int)status}).";
    }

    private static ForecastData Map(WeatherLocation location, OpenMeteoResponse dto)
    {
        var current = dto.Current!;
        var daily = dto.Daily!;

        var data = new ForecastData
        {
            Location = location,
            RetrievedAt = DateTime.UtcNow,
            Current = new CurrentWeather
            {
                Time = ParseTime(current.Time),
                Temperature = current.Temperature,
                ApparentTemperature = current.ApparentTemperature,
                Humidity = RoundOrNull(current.Humidity),
                Precipitation = current.Precipitation,
                WeatherCode = RoundOrNull(current.WeatherCode) ?? -1,
                WindSpeed = current.WindSpeed
            }
        };

        for (var i = 0; i < daily.Time!.Count; i++)
        {
            if (!DateOnly.TryParse(daily.Time[i], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                continue; // skip a day with no usable date
            }

            data.Days.Add(new DailyForecast
            {
                Date = date,
                WeatherCode = RoundOrNull(ValueAt(daily.WeatherCode, i)) ?? -1,
                TempMax = ValueAt(daily.TemperatureMax, i),
                TempMin = ValueAt(daily.TemperatureMin, i),
                PrecipitationChance = RoundOrNull(ValueAt(daily.PrecipitationProbabilityMax, i)),
                WindSpeedMax = ValueAt(daily.WindSpeedMax, i)
            });
        }

        return data;
    }

    // Safe list access: a missing list or a short list gives null instead of an exception.
    private static double? ValueAt(List<double?>? list, int index) =>
        list is not null && index < list.Count ? list[index] : null;

    private static int? RoundOrNull(double? value) =>
        value.HasValue ? (int)Math.Round(value.Value) : null;

    private static DateTime ParseTime(string? value) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)
            ? time
            : DateTime.Now;
}