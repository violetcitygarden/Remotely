using System.Text.Json;

namespace Remotely.Console.Win;

internal sealed record ServerSettings(string ServerUrl)
{
    public static ServerSettings Default { get; } = new("https://localhost:5001");
}

internal sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsPath;

    public SettingsStore()
    {
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Remotely Console");

        _settingsPath = Path.Combine(settingsDirectory, "settings.json");
    }

    public ServerSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return ServerSettings.Default;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<ServerSettings>(json);
            return settings is null || !ServerUrl.TryNormalize(settings.ServerUrl, out var normalized)
                ? ServerSettings.Default
                : new ServerSettings(normalized);
        }
        catch (JsonException)
        {
            return ServerSettings.Default;
        }
        catch (IOException)
        {
            return ServerSettings.Default;
        }
    }

    public void Save(ServerSettings settings)
    {
        var directory = Path.GetDirectoryName(_settingsPath)
            ?? throw new InvalidOperationException("Não foi possível determinar a pasta de configurações.");

        Directory.CreateDirectory(directory);
        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }
}

internal static class ServerUrl
{
    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value) ||
            !Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return false;
        }

        normalized = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        return true;
    }
}
