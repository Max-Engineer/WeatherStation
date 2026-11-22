using System.Text.Json.Serialization;

namespace WeatherStation.Models;

public class OpenMeteoResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("current")]
    public OpenMeteoCurrent? Current { get; set; }

    [JsonPropertyName("daily")]
    public OpenMeteoDaily? Daily { get; set; }
}

public class OpenMeteoCurrent
{
    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("temperature_2m")]
    public double? Temperature { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public double? Humidity { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public double? ApparentTemperature { get; set; }

    [JsonPropertyName("precipitation")]
    public double? Precipitation { get; set; }

    [JsonPropertyName("weather_code")]
    public double? WeatherCode { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double? WindSpeed { get; set; }
}

// Daily data arrives as parallel lists: index 0 of every list is day 1, and so on.
public class OpenMeteoDaily
{
    [JsonPropertyName("time")]
    public List<string?>? Time { get; set; }

    [JsonPropertyName("weather_code")]
    public List<double?>? WeatherCode { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public List<double?>? TemperatureMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public List<double?>? TemperatureMin { get; set; }

    [JsonPropertyName("precipitation_probability_max")]
    public List<double?>? PrecipitationProbabilityMax { get; set; }

    [JsonPropertyName("wind_speed_10m_max")]
    public List<double?>? WindSpeedMax { get; set; }
}