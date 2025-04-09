using RpaJsonDllStudio.Models;
using System;
using System.Threading.Tasks;

namespace RpaJsonDllStudio.Services
{
    /// <summary>
    /// Интерфейс сервиса для управления настройками сборки
    /// </summary>
    public interface IBuildSettingsService
    {
        /// <summary>
        /// Текущие настройки сборки
        /// </summary>
        BuildSettings CurrentSettings { get; }
        
        /// <summary>
        /// Событие изменения настроек сборки
        /// </summary>
        event EventHandler<BuildSettings> SettingsChanged;
        
        /// <summary>
        /// Загрузить настройки сборки
        /// </summary>
        /// <returns>Загруженные настройки</returns>
        Task<BuildSettings> LoadSettingsAsync();
        
        /// <summary>
        /// Сохранить настройки сборки
        /// </summary>
        /// <param name="settings">Настройки для сохранения</param>
        /// <returns>True, если сохранение выполнено успешно</returns>
        Task<bool> SaveSettingsAsync(BuildSettings settings);
        
        /// <summary>
        /// Обновить настройки сборки
        /// </summary>
        /// <param name="settings">Новые настройки</param>
        void UpdateSettings(BuildSettings settings);
        
        /// <summary>
        /// Добавить ссылку на библиотеку
        /// </summary>
        /// <param name="referencePath">Путь к библиотеке</param>
        void AddReference(string referencePath);
        
        /// <summary>
        /// Удалить ссылку на библиотеку
        /// </summary>
        /// <param name="referencePath">Путь к библиотеке</param>
        void RemoveReference(string referencePath);
        
        /// <summary>
        /// Установить путь к DLL
        /// </summary>
        /// <param name="dllPath">Путь к DLL</param>
        void SetDllPath(string dllPath);
    }
} 