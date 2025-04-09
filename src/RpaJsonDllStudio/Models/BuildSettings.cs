using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RpaJsonDllStudio.Models
{
    /// <summary>
    /// Класс, представляющий настройки сборки DLL и используемых библиотек
    /// </summary>
    public class BuildSettings
    {
        /// <summary>
        /// Путь к последней использованной DLL
        /// </summary>
        public string LastDllPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Название сборки (библиотеки)
        /// </summary>
        public string AssemblyName { get; set; } = "RpaJson.Generated";
        
        /// <summary>
        /// Список ссылок на библиотеки, использованные при сборке DLL
        /// </summary>
        public List<string> References { get; set; } = new List<string>();
        
        /// <summary>
        /// Базовое пространство имён для генерируемых классов
        /// </summary>
        public string BaseNamespace { get; set; } = "RpaJsonGenerated";
        
        /// <summary>
        /// Флаг использования сильной типизации
        /// </summary>
        public bool UseStrongTyping { get; set; } = true;
        
        /// <summary>
        /// Флаг добавления JSON аннотаций в классы
        /// </summary>
        public bool AddJsonAnnotations { get; set; } = true;
        
        /// <summary>
        /// Дата последнего сохранения настроек
        /// </summary>
        [JsonIgnore]
        public DateTime LastSaved { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Создать копию настроек
        /// </summary>
        public BuildSettings Clone()
        {
            return new BuildSettings
            {
                LastDllPath = LastDllPath,
                AssemblyName = AssemblyName,
                References = [..References],
                BaseNamespace = BaseNamespace,
                UseStrongTyping = UseStrongTyping,
                AddJsonAnnotations = AddJsonAnnotations,
                LastSaved = LastSaved
            };
        }
    }
} 