using System;
using System.Collections.Generic;

namespace RpaJsonDllStudio.Models;

/// <summary>
/// Настройки генерации кода и компиляции DLL
/// </summary>
public class CompilationSettings
{
    /// <summary>
    /// Целевой фреймворк компиляции
    /// </summary>
    public TargetFramework TargetFramework { get; set; } = TargetFramework.NetStandard20;
        
    /// <summary>
    /// Используемая библиотека JSON
    /// </summary>
    public JsonLibrary JsonLibrary { get; set; } = JsonLibrary.NewtonsoftJson;
        
    /// <summary>
    /// Пространство имен для генерируемых классов
    /// </summary>
    public string Namespace { get; set; } = "RpaJsonModels";
    
    /// <summary>
    /// Имя корневого класса
    /// </summary>
    public string RootClassName { get; set; } = "Root";
        
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
    public bool GenerateDefaultConstructor { get; set; } = true;
        
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
    /// Путь для сохранения DLL
    /// </summary>
    public string OutputPath { get; set; } = "";
        
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
            TargetFramework.Net60 => "net6.0",
            TargetFramework.Net70 => "net7.0",
            TargetFramework.Net80 => "net8.0",
            TargetFramework.Net90 => "net9.0",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
        
    /// <summary>
    /// Получить список ссылок на сборки, необходимые для компиляции
    /// </summary>
    public List<string> GetRequiredAssemblies()
    {
        var assemblies = new List<string>
        {
            "System.dll",
            "System.Core.dll",
            "System.Collections.dll",
            "mscorlib.dll"
        };

        switch (JsonLibrary)
        {
            // Добавляем сборки в зависимости от выбранной библиотеки JSON
            case JsonLibrary.NewtonsoftJson:
                assemblies.Add("Newtonsoft.Json.dll");
                break;
            case JsonLibrary.SystemTextJson:
                assemblies.Add("System.Text.Json.dll");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
            
        return assemblies;
    }
}