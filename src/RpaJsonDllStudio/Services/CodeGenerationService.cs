using Newtonsoft.Json.Linq;
using RpaJsonDllStudio.Models;
using RpaJsonDllStudio.Services.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaJsonDllStudio.Services;

/// <summary>
/// Реализация сервиса для генерации C# кода из JSON
/// </summary>
public class CodeGenerationService : ICodeGenerationService
{
    private readonly CodeGenerator _codeGenerator;
    private readonly CodeCompiler _codeCompiler;

    /// <summary>
    /// Конструктор с внедрением зависимостей
    /// </summary>
    public CodeGenerationService(
        CodeGenerator codeGenerator,
        CodeCompiler codeCompiler)
    {
        _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        _codeCompiler = codeCompiler ?? throw new ArgumentNullException(nameof(codeCompiler));
    }
    
    /// <summary>
    /// Генерирует C# код из JSON строки
    /// </summary>
    public Task<string> GenerateCodeFromJsonAsync(string json, CompilationSettings settings)
    {
        return _codeGenerator.GenerateCodeFromJsonAsync(json, settings);
    }

    /// <summary>
    /// Компилирует C# код в DLL
    /// </summary>
    public Task<string?> CompileToDllAsync(string csharpCode, string outputPath, CompilationSettings settings)
    {
        return _codeCompiler.CompileToDllAsync(csharpCode, outputPath, settings);
    }

    /// <summary>
    /// Проверяет валидность C# кода
    /// </summary>
    public Task<string[]> ValidateCSharpCodeAsync(string csharpCode, CompilationSettings settings)
    {
        return _codeCompiler.ValidateCSharpCodeAsync(csharpCode, settings);
    }
} 