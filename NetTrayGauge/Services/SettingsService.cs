using System;
using System.IO;
using System.Text.Json;
using NetTrayGauge.Models;

namespace NetTrayGauge.Services;

/// <summary>
/// Loads and saves persisted settings.
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private readonly LoggingService _logger;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
    {
        WriteIndented = true
    };

    public SettingsService(string appFolder, LoggingService logger)
    {
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "settings.json");
        _logger = logger;
    }

    public Settings Current { get; private set; } = new();

    public void Load()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                Current = JsonSerializer.Deserialize<Settings>(json, _options) ?? new Settings();
            }
            catch
            {
                Current = new Settings();
            }
        }
        else
        {
            Current = new Settings();
            Save();
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(Current, _options);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save settings to '{_settingsPath}'", ex);
        }
    }

    public void Update(Action<Settings> update)
    {
        update(Current);
        Save();
    }

    public void Export(string destination)
    {
        File.Copy(_settingsPath, destination, true);
    }

    public void Import(string source)
    {
        File.Copy(source, _settingsPath, true);
        Load();
    }
}
