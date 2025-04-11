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
    }
} 