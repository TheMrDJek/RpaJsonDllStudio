using Newtonsoft.Json.Linq;
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
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace RpaJsonDllStudio.Services;

/// <summary>
/// Реализация сервиса для генерации C# кода из JSON
/// </summary>
public class CodeGenerationService : ICodeGenerationService
{
    /// <summary>
    /// Генерирует C# код из JSON строки
    /// </summary>
    public async Task<string> GenerateCodeFromJsonAsync(string json, CompilationSettings settings)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON не может быть пустым", nameof(json));

        // Проверяем валидность JSON
        if (!IsValidJson(json))
            throw new ArgumentException("Неверный формат JSON", nameof(json));

        // Выполняем генерацию асинхронно для предотвращения блокировки UI
        return await Task.Run(() =>
        {
            var rootObject = JToken.Parse(json);
            var sb = new StringBuilder();
            var rootClassBuilder = new StringBuilder();
            var otherClassesBuilder = new StringBuilder();
                
            // Добавляем необходимые using директивы
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.Serialization;");
            sb.AppendLine("using System.ComponentModel;");
            sb.AppendLine("using System.Linq;");

            if (settings.JsonLibrary == JsonLibrary.NewtonsoftJson)
            {
                sb.AppendLine("using Newtonsoft.Json;");
                sb.AppendLine("using Newtonsoft.Json.Serialization;");
            }
            else if (settings.JsonLibrary == JsonLibrary.SystemTextJson)
            {
                sb.AppendLine("using System.Text.Json;");
                sb.AppendLine("using System.Text.Json.Serialization;");
            }
                
            sb.AppendLine();
                
            // Начинаем namespace
            sb.AppendLine($"namespace {settings.Namespace}");
            sb.AppendLine("{");
                
            // Генерируем классы на основе JSON
            // Используем отдельные StringBuilder для Root и других классов
            GenerateClassesFromToken(rootObject, settings.RootClassName, rootClassBuilder, otherClassesBuilder, settings);
                
            // Добавляем сначала Root класс, затем остальные классы
            sb.Append(rootClassBuilder.ToString());
            sb.Append(otherClassesBuilder.ToString());
                
            // Закрываем namespace
            sb.AppendLine("}");
                
            return sb.ToString();
        });
    }

    /// <summary>
    /// Компилирует C# код в DLL
    /// </summary>
    public async Task<string?> CompileToDllAsync(string csharpCode, string outputPath, CompilationSettings settings)
    {
        if (string.IsNullOrWhiteSpace(csharpCode))
            throw new ArgumentException("C# код не может быть пустым", nameof(csharpCode));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Путь для сохранения DLL не может быть пустым", nameof(outputPath));

        // Проверяем наличие SDK для выбранного фреймворка
        if (!IsSdkAvailable(settings.TargetFramework))
        {
            if (settings.TargetFramework == TargetFramework.NetFramework48)
            {
                throw new Exception($".NET Framework 4.8 не установлен. Пожалуйста, установите его с сайта https://dotnet.microsoft.com/download/dotnet-framework");
            }
            // Для .NET Standard ошибки не выдаем
        }

        // Выполняем компиляцию асинхронно для предотвращения блокировки UI
        return await Task.Run(() =>
        {
            try
            {
                // Создаем синтаксическое дерево из кода
                var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
                    
                // Получаем ссылки на сборки
                var references = GetMetadataReferences(settings);
                
                // Определяем параметры компиляции в зависимости от целевого фреймворка
                var parseOptions = new CSharpParseOptions(
                    GetCSharpLanguageVersion(settings.TargetFramework),
                    preprocessorSymbols: new[] { "DEBUG", "TRACE" }
                );
                
                // Создаем компиляцию
                var compilation = CSharpCompilation.Create(
                    $"{settings.Namespace}.dll",
                    new[] { syntaxTree },
                    references,
                    new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: settings.OptimizeOutput ? OptimizationLevel.Release : OptimizationLevel.Debug,
                        warningLevel: 4,
                        // Включаем дополнительные опции для совместимости с .NET Standard/.NET Core
                        allowUnsafe: true,
                        platform: Platform.AnyCpu,
                        deterministic: true
                    )
                );
                    
                // Компилируем и сохраняем DLL
                using var ms = new MemoryStream();
                EmitResult result = compilation.Emit(ms);
                    
                if (!result.Success)
                {
                    // Обработка ошибок компиляции
                    var failures = result.Diagnostics
                        .Where(diagnostic => 
                            diagnostic.IsWarningAsError || 
                            diagnostic.Severity == DiagnosticSeverity.Error);
                    
                    StringBuilder errorMessage = new StringBuilder("Ошибки компиляции:");
                    
                    // Проверяем наличие ошибок, связанных с отсутствием необходимых сборок
                    bool hasMissingAssemblyErrors = failures.Any(d => 
                        d.Id == "CS0012" || // Тип определен в сборке, на которую нет ссылки
                        d.Id == "CS0246" || // Не удалось найти тип или пространство имен
                        d.Id == "CS0234"    // Не удалось найти тип или пространство имен в пространстве имен
                    );
                    
                    if (hasMissingAssemblyErrors)
                    {
                        errorMessage.AppendLine("\n\nВозможно, на компьютере не установлены необходимые компоненты.");
                        
                        if (settings.TargetFramework == TargetFramework.NetFramework48)
                        {
                            errorMessage.AppendLine($"Для компиляции DLL под .NET Framework 4.8 требуется, чтобы эта версия фреймворка была установлена.");
                            errorMessage.AppendLine("Пожалуйста, установите .NET Framework 4.8 с сайта https://dotnet.microsoft.com/download/dotnet-framework");
                        }
                        else
                        {
                            errorMessage.AppendLine($"Для компиляции DLL под {settings.GetTargetFrameworkString()} могут потребоваться дополнительные библиотеки.");
                        }
                    }
                    
                    foreach (var diagnostic in failures)
                    {
                        errorMessage.AppendLine($"\n{diagnostic.Id}: {diagnostic.GetMessage()}");
                        Console.Error.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    }
                    
                    throw new Exception(errorMessage.ToString());
                }
                    
                // Сохраняем DLL
                ms.Seek(0, SeekOrigin.Begin);
                using var fs = new FileStream(outputPath, FileMode.Create);
                ms.CopyTo(fs);
                    
                return outputPath;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Ошибка компиляции: {ex.Message}");
                throw new Exception($"Ошибка компиляции DLL: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Получает версию языка C# в зависимости от целевого фреймворка
    /// </summary>
    private LanguageVersion GetCSharpLanguageVersion(TargetFramework targetFramework)
    {
        return targetFramework switch
        {
            TargetFramework.NetStandard20 => LanguageVersion.CSharp7_3,
            TargetFramework.NetStandard21 => LanguageVersion.CSharp8,
            TargetFramework.NetFramework48 => LanguageVersion.CSharp8,
            _ => LanguageVersion.CSharp7_3
        };
    }

    /// <summary>
    /// Проверяет валидность JSON
    /// </summary>
    public bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JToken.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Проверяет валидность C# кода
    /// </summary>
    public async Task<List<string>> ValidateCSharpCodeAsync(string csharpCode, CompilationSettings settings)
    {
        if (string.IsNullOrWhiteSpace(csharpCode))
            return new List<string> { "C# код не может быть пустым" };

        // Выполняем проверку асинхронно для предотвращения блокировки UI
        return await Task.Run(() =>
        {
            try
            {
                var errors = new List<string>();
                
                // Создаем синтаксическое дерево из кода
                var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
                
                // Получаем ссылки на сборки
                var references = GetMetadataReferences(settings);
                
                // Определяем параметры компиляции
                var parseOptions = new CSharpParseOptions(
                    GetCSharpLanguageVersion(settings.TargetFramework),
                    preprocessorSymbols: new[] { "DEBUG", "TRACE" }
                );
                
                // Создаем компиляцию
                var compilation = CSharpCompilation.Create(
                    $"{settings.Namespace}.dll",
                    new[] { syntaxTree },
                    references,
                    new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: settings.OptimizeOutput ? OptimizationLevel.Release : OptimizationLevel.Debug,
                        warningLevel: 4,
                        // Включаем дополнительные опции для совместимости с .NET Standard/.NET Core
                        allowUnsafe: true,
                        platform: Platform.AnyCpu,
                        deterministic: true
                    )
                );
                
                // Получаем диагностику (ошибки и предупреждения)
                var diagnostics = compilation.GetDiagnostics();
                
                // Фильтруем только ошибки и предупреждения, которые обрабатываются как ошибки
                var failures = diagnostics.Where(diagnostic => 
                    diagnostic.IsWarningAsError || 
                    diagnostic.Severity == DiagnosticSeverity.Error);
                
                if (failures.Any())
                {
                    foreach (var diagnostic in failures)
                    {
                        errors.Add($"[{diagnostic.Id}] {diagnostic.GetMessage()} (Строка {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1})");
                    }
                }
                
                return errors;
            }
            catch (Exception ex)
            {
                return new List<string> { $"Ошибка проверки кода: {ex.Message}" };
            }
        });
    }

    #region Private Methods

    /// <summary>
    /// Очищает строку от HTML-сущностей и заменяет лишние пробелы символом подчеркивания
    /// </summary>
    private string CleanupPropertyName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "EmptyProperty";
            
        // Заменяем HTML неразрывные пробелы на символ подчеркивания
        string result = name.Replace("&nbsp;", "_");
        
        // Заменяем последовательности обычных пробелов на одиночный символ подчеркивания
        result = Regex.Replace(result, @"\s+", "_");
        
        // Удаляем все недопустимые символы для имен C# (оставляем только буквы, цифры и подчеркивания)
        result = Regex.Replace(result, @"[^\w]", "_");
        
        // Если имя начинается с цифры, добавляем префикс
        if (char.IsDigit(result[0]))
            result = "Prop_" + result;
            
        return result;
    }

    /// <summary>
    /// Генерирует классы C# на основе JSON структуры
    /// </summary>
    private void GenerateClassesFromToken(JToken token, string className, StringBuilder rootSb, StringBuilder otherClassesSb, CompilationSettings settings, int indentLevel = 1)
    {
        // Генерация только поддерживаемых типов: объекты, массивы, примитивы
        if (token is JObject obj)
        {
            // Удаляем префикс Root из имени класса, если он есть
            string actualClassName = className;
            if (className != settings.RootClassName && className.StartsWith(settings.RootClassName))
            {
                actualClassName = className.Substring(settings.RootClassName.Length);
            }
            
            // Создаем StringBuilder для текущего класса
            var currentClassSb = new StringBuilder();
            
            // Генерируем класс для объекта
            string indent = new string(' ', indentLevel * 4);
                
            // Добавляем XML документацию, если нужно
            if (settings.GenerateXmlDocumentation)
            {
                currentClassSb.AppendLine($"{indent}/// <summary>");
                currentClassSb.AppendLine($"{indent}/// Класс, представляющий {actualClassName}");
                currentClassSb.AppendLine($"{indent}/// </summary>");
            }
                
            currentClassSb.AppendLine($"{indent}public class {actualClassName}");
            currentClassSb.AppendLine($"{indent}{{");
            
            // Пропускаем генерацию конструкторов
                
            // Генерируем свойства
            int propertyCount = obj.Properties().Count();
            int currentPropertyIndex = 0;
            
            foreach (var property in obj.Properties())
            {
                currentPropertyIndex++;
                
                // Очищаем имя свойства от HTML-сущностей и лишних пробелов
                string originalPropertyName = property.Name;
                string cleanPropertyName = CleanupPropertyName(originalPropertyName);
                
                string propertyName = cleanPropertyName;
                if (settings.UsePascalCase && !string.IsNullOrEmpty(propertyName))
                {
                    // Преобразуем первую букву в верхний регистр для PascalCase
                    propertyName = char.ToUpper(propertyName[0]) + propertyName.Substring(1);
                }
                    
                var propertyValue = property.Value;
                
                // Для массивов используем другое именование (без повторения имени родительского класса)
                string childClassName;
                if (propertyValue is JArray && propertyName.EndsWith("s"))
                {
                    // Если имя свойства заканчивается на 's', используем единственное число
                    childClassName = propertyName.Substring(0, propertyName.Length - 1);
                }
                else
                {
                    childClassName = $"{actualClassName}{propertyName}";
                }
                
                string propertyType = GetCSharpType(propertyValue, childClassName, settings);
                    
                // Добавляем атрибут JsonProperty, если нужно
                if (settings.GenerateJsonPropertyAttributes)
                {
                    if (settings.JsonLibrary == JsonLibrary.NewtonsoftJson)
                    {
                        currentClassSb.AppendLine($"{indent}    [JsonProperty(\"{originalPropertyName}\")]");
                    }
                    else if (settings.JsonLibrary == JsonLibrary.SystemTextJson)
                    {
                        currentClassSb.AppendLine($"{indent}    [JsonPropertyName(\"{originalPropertyName}\")]");
                    }
                }
                    
                // Добавляем XML документацию для свойства, если нужно
                if (settings.GenerateXmlDocumentation)
                {
                    currentClassSb.AppendLine($"{indent}    /// <summary>");
                    currentClassSb.AppendLine($"{indent}    /// Свойство {propertyName}");
                    currentClassSb.AppendLine($"{indent}    /// </summary>");
                }
                    
                // Генерируем свойство
                if (settings.GenerateProperties)
                {
                    currentClassSb.AppendLine($"{indent}    public {propertyType} {propertyName} {{ get; set; }}");
                }
                else
                {
                    currentClassSb.AppendLine($"{indent}    public {propertyType} {propertyName};");
                }
                
                // Добавляем пустую строку между свойствами для лучшей читабельности
                if (currentPropertyIndex < propertyCount)
                {
                    currentClassSb.AppendLine();
                }
                    
                // Если свойство - объект или массив объектов, генерируем соответствующий класс
                if (propertyValue is JObject)
                {
                    GenerateClassesFromToken(propertyValue, childClassName, null, otherClassesSb, settings, indentLevel);
                }
                else if (propertyValue is JArray array && array.Count > 0 && array[0] is JObject)
                {
                    // Для массива объектов генерируем класс с именем элемента (без Item на конце)
                    string itemClassName;
                    // Если имя свойства заканчивается на 's', используем единственное число для типа
                    if (propertyName.EndsWith("s"))
                    {
                        itemClassName = propertyName.Substring(0, propertyName.Length - 1);
                    }
                    else
                    {
                        itemClassName = $"{childClassName}Item";
                    }
                    GenerateClassesFromToken(array[0], itemClassName, null, otherClassesSb, settings, indentLevel);
                }
            }
                
            currentClassSb.AppendLine($"{indent}}}");
            
            // Если это корневой класс, добавляем его в rootSb
            if (className == settings.RootClassName)
            {
                rootSb.AppendLine(currentClassSb.ToString());
            }
            else
            {
                // Добавляем другие классы в otherClassesSb
                otherClassesSb.AppendLine(currentClassSb.ToString());
            }
        }
        else if (token is JArray array && array.Count > 0)
        {
            // Если массив содержит объекты, генерируем класс для первого элемента
            if (array[0] is JObject firstObject)
            {
                GenerateClassesFromToken(firstObject, className, rootSb, otherClassesSb, settings, indentLevel);
            }
        }
    }

    /// <summary>
    /// Определяет C# тип для JSON значения
    /// </summary>
    private string GetCSharpType(JToken token, string className, CompilationSettings settings)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
                return "int";
            case JTokenType.Float:
                return "double";
            case JTokenType.String:
                return "string";
            case JTokenType.Boolean:
                return "bool";
            case JTokenType.Date:
                return "DateTime";
            case JTokenType.Null:
                return "object";
            case JTokenType.Object:
                // Удаляем префикс Root из имени типа, если он есть
                string actualClassName = className;
                if (className != settings.RootClassName && className.StartsWith(settings.RootClassName))
                {
                    actualClassName = className.Substring(settings.RootClassName.Length);
                }
                return actualClassName;
            case JTokenType.Array:
                var array = (JArray)token;
                if (array.Count == 0)
                    return "object[]";
                
                if (array[0] is JObject)
                {
                    // Удаляем префикс Root из имени типа элементов массива, если он есть
                    string actualElementClassName = className;
                    if (className != settings.RootClassName && className.StartsWith(settings.RootClassName))
                    {
                        actualElementClassName = className.Substring(settings.RootClassName.Length);
                    }
                    return $"{actualElementClassName}[]";
                }
                else
                {
                    var elementType = GetCSharpType(array[0], className, settings);
                    return $"{elementType}[]";
                }
                
            default:
                return "object";
        }
    }

    /// <summary>
    /// Получает ссылки на сборки для компиляции
    /// </summary>
    private List<MetadataReference> GetMetadataReferences(CompilationSettings settings)
    {
        var references = new List<MetadataReference>();
            
        // Базовые сборки .NET
        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Console).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(System.Collections.IEnumerable).Assembly.Location));
        
        // Добавляем System.Runtime.dll - необходима для Attribute и других базовых типов
        var runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "System.Runtime");
        if (runtimeAssembly != null)
        {
            references.Add(MetadataReference.CreateFromFile(runtimeAssembly.Location));
        }
        
        // Добавляем ссылку на MSCorLib
        string mscorlibLocation = typeof(object).Assembly.Location;
        string runtimeDirectory = Path.GetDirectoryName(mscorlibLocation);
        
        if (!string.IsNullOrEmpty(runtimeDirectory))
        {
            // Добавляем ссылки на другие базовые библиотеки .NET
            string[] frameworkAssemblies = {
                "System.Runtime.dll",
                "System.Private.CoreLib.dll",
                "System.ObjectModel.dll",
                "System.Linq.dll",
                "System.Collections.dll"
            };
            
            foreach (var assembly in frameworkAssemblies)
            {
                string assemblyPath = Path.Combine(runtimeDirectory, assembly);
                if (File.Exists(assemblyPath))
                {
                    references.Add(MetadataReference.CreateFromFile(assemblyPath));
                }
            }
        }
            
        // Сборки для библиотек JSON
        if (settings.JsonLibrary == JsonLibrary.NewtonsoftJson)
        {
            // Находим пути ко всем сборкам Newtonsoft.Json
            var newtonsoftAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Newtonsoft.Json");
                
            if (newtonsoftAssembly != null)
            {
                references.Add(MetadataReference.CreateFromFile(newtonsoftAssembly.Location));
            }
        }
        else if (settings.JsonLibrary == JsonLibrary.SystemTextJson)
        {
            // Находим пути ко всем сборкам System.Text.Json
            var systemTextJsonAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "System.Text.Json");
                
            if (systemTextJsonAssembly != null)
            {
                references.Add(MetadataReference.CreateFromFile(systemTextJsonAssembly.Location));
            }
        }
            
        return references;
    }

    /// <summary>
    /// Проверяет наличие SDK для выбранного фреймворка
    /// </summary>
    private bool IsSdkAvailable(TargetFramework targetFramework)
    {
        try
        {
            // Проверка только для .NET Framework
            if (targetFramework == TargetFramework.NetFramework48)
            {
                // Проверяем наличие сборок .NET Framework 4.8
                string netfxPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319";
                return Directory.Exists(netfxPath);
            }
            
            // Для .NET Standard проверку не выполняем
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