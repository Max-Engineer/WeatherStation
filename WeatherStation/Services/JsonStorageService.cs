using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherStation.Services;

public interface IJsonStorageService
{
    // Returns a new T if the file is missing or damaged. Never throws for bad files.
    Task<T> LoadAsync<T>(string fileName) where T : new();

    // Returns false if the file could not be saved.
    Task<bool> SaveAsync<T>(string fileName, T data);
}

public class JsonStorageService : IJsonStorageService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _directory;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonStorageService(string directory)
    {
        _directory = directory;
    }

    public async Task<T> LoadAsync<T>(string fileName) where T : new()
    {
        var path = Path.Combine(_directory, fileName);

        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(path))
            {
                return new T(); // first run
            }

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(json, Options) ?? new T();
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"{fileName} is damaged: {ex.Message}");
            KeepDamagedCopy(path);
            return new T();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Debug.WriteLine($"Could not read {fileName}: {ex.Message}");
            return new T();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> SaveAsync<T>(string fileName, T data)
    {
        var path = Path.Combine(_directory, fileName);
        var tempPath = path + ".tmp";

        await _lock.WaitAsync();
        try
        {
            Directory.CreateDirectory(_directory);

            // Write to a temp file first, then swap it in, so a crash can't leave half a file.
            var json = JsonSerializer.Serialize(data, Options);
            await File.WriteAllTextAsync(tempPath, json);
            File.Move(tempPath, path, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            Debug.WriteLine($"Could not save {fileName}: {ex.Message}");
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static void KeepDamagedCopy(string path)
    {
        try
        {
            File.Move(path, path + ".damaged", overwrite: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not rename damaged file: {ex.Message}");
        }
    }
}