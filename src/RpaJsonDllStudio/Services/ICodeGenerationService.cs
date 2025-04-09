using RpaJsonDllStudio.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaJsonDllStudio.Services;

/// <summary>
/// Интерфейс сервиса для генерации C# кода из JSON
/// </summary>
public interface ICodeGenerationService
{
    /// <summary>
    /// Генерирует C# код из JSON строки
    /// </summary>
    /// <param name="json">JSON строка</param>
    /// <param name="settings">Настройки генерации</param>
    /// <returns>Сгенерированный C# код</returns>
    Task<string> GenerateCodeFromJsonAsync(string json, CompilationSettings settings);
        
    /// <summary>
    /// Компилирует C# код в DLL
    /// </summary>
    /// <param name="csharpCode">C# код</param>
    /// <param name="outputPath">Путь для сохранения DLL</param>
    /// <param name="settings">Настройки компиляции</param>
    /// <returns>Путь к скомпилированной DLL или null в случае ошибки</returns>
    Task<string?> CompileToDllAsync(string csharpCode, string outputPath, CompilationSettings settings);
    
    /// <summary>
    /// Проверяет валидность C# кода
    /// </summary>
    /// <param name="csharpCode">C# код для проверки</param>
    /// <param name="settings">Настройки компиляции</param>
    /// <returns>Результат проверки: пустой список если ошибок нет, или список ошибок компиляции</returns>
    Task<List<string>> ValidateCSharpCodeAsync(string csharpCode, CompilationSettings settings);
        
    /// <summary>
    /// Проверяет валидность JSON
    /// </summary>
    /// <param name="json">JSON строка для проверки</param>
    /// <returns>True если JSON валиден, иначе False</returns>
    bool IsValidJson(string json);
}