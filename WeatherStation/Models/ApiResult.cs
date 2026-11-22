using System.Diagnostics.CodeAnalysis;

namespace WeatherStation.Models;

public class ApiResult<T>
{
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }

    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsSuccess => ErrorMessage is null && Data is not null;

    public static ApiResult<T> Success(T data) => new() { Data = data };

    public static ApiResult<T> Failure(string message) => new() { ErrorMessage = message };
}