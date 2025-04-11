using RpaJsonDllStudio.Models;
using Serilog;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RpaJsonDllStudio.Services;

/// <summary>
/// Реализация сервиса для управления настройками сборки
/// </summary>
public class BuildSettingsService : IBuildSettingsService
{
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RpaJsonDllStudio",
        "build_settings.json");

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public BuildSettings CurrentSettings { get; private set; } = new();

    public BuildSettingsService()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
    }

    public async Task<BuildSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                var settings = JsonSerializer.Deserialize<BuildSettings>(json, _jsonSerializerOptions);

                if (settings != null)
                {
                    CurrentSettings = settings;
                    settings.LastSaved = File.GetLastWriteTime(_settingsPath);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при загрузке настроек");
        }

        return CurrentSettings;
    }

    public async Task<bool> SaveSettingsAsync(BuildSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonSerializerOptions);

            await File.WriteAllTextAsync(_settingsPath, json);

            settings.LastSaved = DateTime.Now;
            CurrentSettings = settings;

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при сохранении настроек");
            return false;
        }
    }

    public void UpdateSettings(BuildSettings settings) => CurrentSettings = settings.Clone();

    public void AddReference(string referencePath)
    {
        if (string.IsNullOrWhiteSpace(referencePath) == false &&
            CurrentSettings.References.Contains(referencePath) == false)
        {
            CurrentSettings.References.Add(referencePath);
        }
    }

    public void RemoveReference(string referencePath) => CurrentSettings.References.Remove(referencePath);

    public void SetDllPath(string dllPath)
    {
        if (CurrentSettings.LastDllPath != dllPath)
        {
            CurrentSettings.LastDllPath = dllPath;
        }
    }
}