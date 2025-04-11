using System;

namespace RpaJsonDllStudio.Models;

/// <summary>
/// Настройки генерации кода и компиляции DLL
/// </summary>
public class CompilationSettings
{
    /// <summary>
    /// Целевой фреймворк компиляции
    /// </summary>
    public TargetFramework TargetFramework { get; set; } = TargetFramework.NetStandard21;
        
    /// <summary>
    /// Используемая библиотека JSON
    /// </summary>
    public JsonLibrary JsonLibrary { get; set; } = JsonLibrary.SystemTextJson;
        
    /// <summary>
    /// Пространство имен для генерируемых классов
    /// </summary>
    public string Namespace { get; set; } = "RpaJsonModels";
    
    /// <summary>
    /// Имя корневого класса
    /// </summary>
    public string RootClassName { get; init; } = "Root";
        
    /// <summary>
    /// Использовать PascalCase для названий свойств
    /// </summary>
    public bool UsePascalCase { get; set; } = true;
        
    /// <summary>
    /// Генерировать свойства вместо полей
    /// </summary>
    public bool GenerateProperties { get; set; } = true;
        
    /// <summary>
    /// Генерировать конструктор по умолчанию
    /// </summary>
    public bool GenerateDefaultConstructor { get; set; }
        
    /// <summary>
    /// Генерировать аннотации JsonProperty
    /// </summary>
    public bool GenerateJsonPropertyAttributes { get; set; } = true;
        
    /// <summary>
    /// Оптимизировать скомпилированную DLL
    /// </summary>
    public bool OptimizeOutput { get; set; } = true;
        
    /// <summary>
    /// Добавлять XML документацию
    /// </summary>
    public bool GenerateXmlDocumentation { get; set; }
        
    /// <summary>
    /// Строковое представление версии целевого фреймворка
    /// </summary>
    public string GetTargetFrameworkString()
    {
        return TargetFramework switch
        {
            TargetFramework.NetStandard20 => "netstandard2.0",
            TargetFramework.NetStandard21 => "netstandard2.1",
            TargetFramework.NetFramework48 => "net48",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}