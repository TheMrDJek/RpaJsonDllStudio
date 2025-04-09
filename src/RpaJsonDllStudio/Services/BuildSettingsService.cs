using RpaJsonDllStudio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RpaJsonDllStudio.Services
{
    /// <summary>
    /// Реализация сервиса для управления настройками сборки
    /// </summary>
    public class BuildSettingsService : IBuildSettingsService
    {
        private BuildSettings _currentSettings;
        private readonly string _settingsPath;
        
        /// <inheritdoc/>
        public BuildSettings CurrentSettings => _currentSettings;
        
        /// <inheritdoc/>
        public event EventHandler<BuildSettings> SettingsChanged;
        
        /// <summary>
        /// Конструктор сервиса
        /// </summary>
        public BuildSettingsService()
        {
            _currentSettings = new BuildSettings();
            
            // Путь к файлу настроек в подпапке данных приложения
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RpaJsonDllStudio"
            );
            
            // Создаем директорию, если она не существует
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            _settingsPath = Path.Combine(appDataPath, "build_settings.json");
        }
        
        /// <inheritdoc/>
        public async Task<BuildSettings> LoadSettingsAsync()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = await File.ReadAllTextAsync(_settingsPath);
                    var settings = JsonSerializer.Deserialize<BuildSettings>(json);
                    
                    if (settings != null)
                    {
                        _currentSettings = settings;
                        settings.LastSaved = File.GetLastWriteTime(_settingsPath);
                        SettingsChanged?.Invoke(this, _currentSettings);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
            }
            
            return _currentSettings;
        }
        
        /// <inheritdoc/>
        public async Task<bool> SaveSettingsAsync(BuildSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                await File.WriteAllTextAsync(_settingsPath, json);
                
                settings.LastSaved = DateTime.Now;
                _currentSettings = settings;
                SettingsChanged?.Invoke(this, _currentSettings);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public void UpdateSettings(BuildSettings settings)
        {
            _currentSettings = settings.Clone();
            SettingsChanged?.Invoke(this, _currentSettings);
        }
        
        /// <inheritdoc/>
        public void AddReference(string referencePath)
        {
            if (!string.IsNullOrWhiteSpace(referencePath) && 
                !_currentSettings.References.Contains(referencePath))
            {
                _currentSettings.References.Add(referencePath);
                SettingsChanged?.Invoke(this, _currentSettings);
            }
        }
        
        /// <inheritdoc/>
        public void RemoveReference(string referencePath)
        {
            if (_currentSettings.References.Contains(referencePath))
            {
                _currentSettings.References.Remove(referencePath);
                SettingsChanged?.Invoke(this, _currentSettings);
            }
        }
        
        /// <inheritdoc/>
        public void SetDllPath(string dllPath)
        {
            if (_currentSettings.LastDllPath != dllPath)
            {
                _currentSettings.LastDllPath = dllPath;
                SettingsChanged?.Invoke(this, _currentSettings);
            }
        }
    }
} 