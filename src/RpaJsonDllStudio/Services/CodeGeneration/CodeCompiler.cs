using RpaJsonDllStudio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Serilog;

namespace RpaJsonDllStudio.Services.CodeGeneration;

/// <summary>
/// Класс для компиляции C# кода в DLL
/// </summary>
public class CodeCompiler
{
    private static readonly string[] BaseAssemblies =
    [
        "System.Runtime.dll",
        "System.Private.CoreLib.dll",
        "System.ObjectModel.dll",
        "System.Linq.dll",
        "System.Collections.dll"
    ];

    private static readonly string[] MissingAssemblyErrors =
    [
        "CS0012", // Тип определен в сборке, на которую нет ссылки
        "CS0246", // Не удалось найти тип или пространство имен
        "CS0234" // Не удалось найти тип или пространство имен в пространстве имен
    ];
    
    private static readonly Dictionary<string, MetadataReference> MetadataCache = new();
    
    private static readonly Dictionary<string, CSharpCompilationOptions> OptionsCache = new();

    #region Публичные методы

    /// <summary>
    /// Компилирует C# код в DLL
    /// </summary>
    public async Task<string?> CompileToDllAsync(string csharpCode, string outputPath, CompilationSettings settings)
    {
        ValidateCompilationInputs(csharpCode, outputPath, settings);

        // Выполняем компиляцию асинхронно для предотвращения блокировки UI
        return await Task.Run(() => CompileToDll(csharpCode, outputPath, settings));
    }
    
    /// <summary>
    /// Проверяет валидность C# кода
    /// </summary>
    public async Task<string[]> ValidateCSharpCodeAsync(string csharpCode, CompilationSettings settings)
    {
        if (string.IsNullOrWhiteSpace(csharpCode))
        {
            return ["C# код не может быть пустым"];
        }

        // Выполняем проверку асинхронно для предотвращения блокировки UI
        return await Task.Run(() => ValidateCSharpCode(csharpCode, settings));
    }

    #endregion

    #region Процесс компиляции
    
    private static void ValidateCompilationInputs(string csharpCode, string outputPath, CompilationSettings settings)
    {
        if (string.IsNullOrWhiteSpace(csharpCode))
        {
            throw new ArgumentException("C# код не может быть пустым", nameof(csharpCode));
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Путь для сохранения DLL не может быть пустым", nameof(outputPath));
        }
        
        if (IsSdkAvailable(settings.TargetFramework) == false &&
            settings.TargetFramework == TargetFramework.NetFramework48)
        {
            throw new Exception(
                ".NET Framework 4.8 не установлен." +
                " Пожалуйста, установите его с сайта https://dotnet.microsoft.com/download/dotnet-framework");
            // Для .NET Standard ошибки не выдаем
        }
    }
    
    private string CompileToDll(string csharpCode, string outputPath, CompilationSettings settings)
    {
        try
        {
            var compilation = CreateCompilation(csharpCode, settings);
            
            return EmitDll(compilation, outputPath, settings);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка компиляции DLL");
            throw;
        }
    }

    private CSharpCompilation CreateCompilation(string csharpCode, CompilationSettings settings)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
        
        // Получаем ссылки на сборки
        var references = GetMetadataReferences(settings);
        
        // Получаем или создаем опции компиляции
        var options = GetCompilationOptions(settings);
        
        return CSharpCompilation.Create(
            $"{settings.Namespace}.dll",
            [syntaxTree],
            references,
            options
        );
    }
    
    private static CSharpCompilationOptions GetCompilationOptions(CompilationSettings settings)
    {
        // Создаем ключ кеша на основе настроек
        var cacheKey = $"{settings.OptimizeOutput}";
        
        // Проверяем наличие опций в кеше
        if (OptionsCache.TryGetValue(cacheKey, out var options))
        {
            return options;
        }
        
        // Создаем новые опции
        options = CreateCompilationOptions(settings);
        
        // Сохраняем в кеше
        OptionsCache[cacheKey] = options;
        
        return options;
    }
    
    private static CSharpCompilationOptions CreateCompilationOptions(CompilationSettings settings)
    {
        return new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: settings.OptimizeOutput
                ? OptimizationLevel.Release
                : OptimizationLevel.Debug,
            warningLevel: 4,
            // Включаем дополнительные опции для совместимости с .NET Standard/.NET Core
            allowUnsafe: true,
            platform: Platform.AnyCpu,
            deterministic: true
        );
    }
    
    private string EmitDll(CSharpCompilation compilation, string outputPath, CompilationSettings settings)
    {
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (result.Success == false)
        {
            HandleCompilationErrors(result, settings);
        }

        SaveDllToFile(ms, outputPath);

        return outputPath;
    }
    
    private static void SaveDllToFile(MemoryStream ms, string outputPath)
    {
        ms.Seek(0, SeekOrigin.Begin);
        using var fs = new FileStream(outputPath, FileMode.Create);
        ms.CopyTo(fs);
    }

    #endregion

    #region Валидация кода
    
    private string[] ValidateCSharpCode(string csharpCode, CompilationSettings settings)
    {
        try
        {
            var compilation = CreateCompilation(csharpCode, settings);
            
            return CheckCompilationErrors(compilation);
        }
        catch (Exception ex)
        {
            return [$"Ошибка проверки кода: {ex.Message}"];
        }
    }
    
    private static string[] CheckCompilationErrors(CSharpCompilation compilation)
    {
        // Получаем диагностику (ошибки и предупреждения)
        var diagnostics = compilation.GetDiagnostics();

        // Фильтруем только ошибки и предупреждения, которые обрабатываются как ошибки
        var failures = GetCompilationFailures(diagnostics);

        if (failures.Length > 0)
        {
            return FormatDiagnosticsMessages(failures);
        }

        return [];
    }

    /// <summary>
    /// Получает массив ошибок компиляции
    /// </summary>
    private static Diagnostic[] GetCompilationFailures(IEnumerable<Diagnostic> diagnostics)
    {
        return diagnostics
            .Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
    }

    /// <summary>
    /// Форматирует сообщения об ошибках компиляции
    /// </summary>
    private static string[] FormatDiagnosticsMessages(Diagnostic[] failures)
    {
        return failures
            .Select(diagnostic =>
                $"[{diagnostic.Id}] {diagnostic.GetMessage()} (Строка {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1})")
            .ToArray();
    }

    #endregion

    #region Обработка ошибок компиляции

    /// <summary>
    /// Обрабатывает ошибки компиляции и формирует читаемое сообщение об ошибке
    /// </summary>
    private void HandleCompilationErrors(EmitResult result, CompilationSettings settings)
    {
        // Получаем массив ошибок компиляции
        var failures = GetCompilationFailures(result.Diagnostics);

        // Формируем сообщение об ошибке
        var errorMessage = FormatErrorMessage(failures, settings);

        throw new Exception(errorMessage);
    }

    /// <summary>
    /// Форматирует сообщение об ошибке
    /// </summary>
    private string FormatErrorMessage(Diagnostic[] failures, CompilationSettings settings)
    {
        var errorMessage = new StringBuilder("Ошибки компиляции:");

        // Проверяем наличие ошибок, связанных с отсутствием необходимых сборок
        bool hasMissingAssemblyErrors = failures.Any(d => MissingAssemblyErrors.Contains(d.Id));

        if (hasMissingAssemblyErrors)
        {
            AppendMissingAssemblyErrorInfo(errorMessage, settings);
        }

        // Добавляем информацию о каждой ошибке
        AppendDiagnosticsInfo(errorMessage, failures);

        return errorMessage.ToString();
    }

    /// <summary>
    /// Добавляет информацию об ошибках, связанных с отсутствием сборок
    /// </summary>
    private static void AppendMissingAssemblyErrorInfo(StringBuilder errorMessage, CompilationSettings settings)
    {
        errorMessage.AppendLine("\n\nВозможно, на компьютере не установлены необходимые компоненты.");

        if (settings.TargetFramework == TargetFramework.NetFramework48)
        {
            errorMessage.AppendLine(
                "Для компиляции DLL под .NET Framework 4.8 требуется, чтобы эта версия фреймворка была установлена.");
            errorMessage.AppendLine(
                "Пожалуйста, установите .NET Framework 4.8 с сайта https://dotnet.microsoft.com/download/dotnet-framework");
        }
        else
        {
            errorMessage.AppendLine(
                $"Для компиляции DLL под {settings.GetTargetFrameworkString()} могут потребоваться дополнительные библиотеки.");
        }
    }

    /// <summary>
    /// Добавляет информацию о диагностических сообщениях
    /// </summary>
    private static void AppendDiagnosticsInfo(StringBuilder errorMessage, Diagnostic[] failures)
    {
        foreach (var diagnostic in failures)
        {
            errorMessage.AppendLine($"\n{diagnostic.Id}: {diagnostic.GetMessage()}");
            Log.Error("{DiagnosticId}: {DiagnosticMessage}", diagnostic.Id, diagnostic.GetMessage());
        }
    }

    #endregion

    #region Управление ссылками на сборки

    /// <summary>
    /// Получает ссылки на сборки для компиляции
    /// </summary>
    private MetadataReference[] GetMetadataReferences(CompilationSettings settings)
    {
        var references = new List<MetadataReference>(20); // Примерное количество ссылок

        // Добавляем базовые сборки .NET
        AddBasicNetReferences(references);

        // Добавляем дополнительные сборки из директории среды выполнения
        AddRuntimeDirectoryAssemblies(references);

        // Добавляем сборки для выбранной библиотеки JSON
        AddJsonLibraryReferences(references, settings.JsonLibrary);

        return references.ToArray();
    }

    /// <summary>
    /// Добавляет базовые ссылки на сборки .NET
    /// </summary>
    private static void AddBasicNetReferences(List<MetadataReference> references)
    {
        // Базовые сборки .NET
        AddCachedReference(references, typeof(object).Assembly.Location);
        AddCachedReference(references, typeof(Console).Assembly.Location);
        AddCachedReference(references, typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location);
        AddCachedReference(references, typeof(System.Collections.IEnumerable).Assembly.Location);

        // Добавляем System.Runtime.dll - необходима для Attribute и других базовых типов
        var runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "System.Runtime");

        if (runtimeAssembly != null)
        {
            AddCachedReference(references, runtimeAssembly.Location);
        }
    }
    
    /// <summary>
    /// Добавляет ссылку на сборку с использованием кэша
    /// </summary>
    private static void AddCachedReference(List<MetadataReference> references, string assemblyPath)
    {
        if (MetadataCache.TryGetValue(assemblyPath, out var reference))
        {
            references.Add(reference);
            return;
        }
        
        reference = MetadataReference.CreateFromFile(assemblyPath);
        MetadataCache[assemblyPath] = reference;
        references.Add(reference);
    }

    /// <summary>
    /// Добавляет сборки из директории среды выполнения
    /// </summary>
    private static void AddRuntimeDirectoryAssemblies(List<MetadataReference> references)
    {
        // Добавляем ссылку на MSCorLib
        string mscorlibLocation = typeof(object).Assembly.Location;
        string? runtimeDirectory = Path.GetDirectoryName(mscorlibLocation);

        if (!string.IsNullOrEmpty(runtimeDirectory))
        {
            // Добавляем ссылки на другие базовые библиотеки .NET
            foreach (var assembly in BaseAssemblies)
            {
                string assemblyPath = Path.Combine(runtimeDirectory, assembly);
                if (File.Exists(assemblyPath))
                {
                    AddCachedReference(references, assemblyPath);
                }
            }
        }
    }

    /// <summary>
    /// Добавляет ссылки на сборки для выбранной библиотеки JSON
    /// </summary>
    private static void AddJsonLibraryReferences(List<MetadataReference> references, JsonLibrary jsonLibrary)
    {
        string? assemblyLocation = null;
        
        if (jsonLibrary == JsonLibrary.NewtonsoftJson)
        {
            // Находим пути ко всем сборкам Newtonsoft.Json
            var newtonsoftAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Newtonsoft.Json");

            if (newtonsoftAssembly != null)
            {
                assemblyLocation = newtonsoftAssembly.Location;
            }
        }
        else if (jsonLibrary == JsonLibrary.SystemTextJson)
        {
            // Находим пути ко всем сборкам System.Text.Json
            var systemTextJsonAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "System.Text.Json");

            if (systemTextJsonAssembly != null)
            {
                assemblyLocation = systemTextJsonAssembly.Location;
            }
        }
        
        if (assemblyLocation != null)
        {
            AddCachedReference(references, assemblyLocation);
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Проверяет наличие SDK для выбранного фреймворка
    /// </summary>
    private static bool IsSdkAvailable(TargetFramework targetFramework)
    {
        try
        {
            if (targetFramework == TargetFramework.NetFramework48)
            {
                // Проверяем наличие сборок .NET Framework 4.8
                const string netfxPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319";
                return Directory.Exists(netfxPath);
            }
            
            return true;
        }
        catch
        {
            // В случае ошибки предполагаем, что SDK не установлен
            return false;
        }
    }

    #endregion
}