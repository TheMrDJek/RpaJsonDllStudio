using Newtonsoft.Json;
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

namespace RpaJsonDllStudio.Services
{
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
                
                // Добавляем необходимые using директивы
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections.Generic;");

                if (settings.JsonLibrary == JsonLibrary.NewtonsoftJson)
                {
                    sb.AppendLine("using Newtonsoft.Json;");
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
                GenerateClassesFromToken(rootObject, "Root", sb, settings);
                
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

            // Выполняем компиляцию асинхронно для предотвращения блокировки UI
            return await Task.Run(() =>
            {
                try
                {
                    // Создаем синтаксическое дерево из кода
                    var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
                    
                    // Получаем ссылки на сборки
                    var references = GetMetadataReferences(settings);
                    
                    // Создаем компиляцию
                    var compilation = CSharpCompilation.Create(
                        $"{settings.Namespace}.dll",
                        new[] { syntaxTree },
                        references,
                        new CSharpCompilationOptions(
                            OutputKind.DynamicallyLinkedLibrary,
                            optimizationLevel: settings.OptimizeOutput ? OptimizationLevel.Release : OptimizationLevel.Debug,
                            warningLevel: 4)
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
                        
                        foreach (var diagnostic in failures)
                        {
                            Console.Error.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                        }
                        
                        return null;
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
                    return null;
                }
            });
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

        #region Private Methods

        /// <summary>
        /// Генерирует классы C# на основе JSON структуры
        /// </summary>
        private void GenerateClassesFromToken(JToken token, string className, StringBuilder sb, CompilationSettings settings, int indentLevel = 1)
        {
            // Генерация только поддерживаемых типов: объекты, массивы, примитивы
            if (token is JObject obj)
            {
                // Генерируем класс для объекта
                string indent = new string(' ', indentLevel * 4);
                
                // Добавляем XML документацию, если нужно
                if (settings.GenerateXmlDocumentation)
                {
                    sb.AppendLine($"{indent}/// <summary>");
                    sb.AppendLine($"{indent}/// Класс, представляющий {className}");
                    sb.AppendLine($"{indent}/// </summary>");
                }
                
                sb.AppendLine($"{indent}public class {className}");
                sb.AppendLine($"{indent}{{");
                
                // Генерируем конструктор по умолчанию, если нужно
                if (settings.GenerateDefaultConstructor)
                {
                    sb.AppendLine($"{indent}    public {className}()");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine();
                }
                
                // Генерируем свойства
                foreach (var property in obj.Properties())
                {
                    string propertyName = property.Name;
                    if (settings.UsePascalCase)
                    {
                        propertyName = char.ToUpper(propertyName[0]) + propertyName.Substring(1);
                    }
                    
                    var propertyValue = property.Value;
                    string propertyType = GetCSharpType(propertyValue, $"{className}{propertyName}Class", settings);
                    
                    // Добавляем атрибут JsonProperty, если нужно
                    if (settings.GenerateJsonPropertyAttributes)
                    {
                        if (settings.JsonLibrary == JsonLibrary.NewtonsoftJson)
                        {
                            sb.AppendLine($"{indent}    [JsonProperty(\"{property.Name}\")]");
                        }
                        else if (settings.JsonLibrary == JsonLibrary.SystemTextJson)
                        {
                            sb.AppendLine($"{indent}    [JsonPropertyName(\"{property.Name}\")]");
                        }
                    }
                    
                    // Добавляем XML документацию для свойства, если нужно
                    if (settings.GenerateXmlDocumentation)
                    {
                        sb.AppendLine($"{indent}    /// <summary>");
                        sb.AppendLine($"{indent}    /// Свойство {propertyName}");
                        sb.AppendLine($"{indent}    /// </summary>");
                    }
                    
                    // Генерируем свойство
                    if (settings.GenerateProperties)
                    {
                        sb.AppendLine($"{indent}    public {propertyType} {propertyName} {{ get; set; }}");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}    public {propertyType} {propertyName};");
                    }
                    
                    // Если свойство - объект или массив объектов, генерируем соответствующий класс
                    if (propertyValue is JObject)
                    {
                        sb.AppendLine();
                        GenerateClassesFromToken(propertyValue, $"{className}{propertyName}Class", sb, settings, indentLevel + 1);
                    }
                    else if (propertyValue is JArray array && array.Count > 0 && array[0] is JObject)
                    {
                        sb.AppendLine();
                        GenerateClassesFromToken(array[0], $"{className}{propertyName}Class", sb, settings, indentLevel + 1);
                    }
                }
                
                sb.AppendLine($"{indent}}}");
            }
            else if (token is JArray array && array.Count > 0)
            {
                // Если массив содержит объекты, генерируем класс для первого элемента
                if (array[0] is JObject firstObject)
                {
                    GenerateClassesFromToken(firstObject, className, sb, settings, indentLevel);
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
                    return className;
                case JTokenType.Array:
                    var array = (JArray)token;
                    if (array.Count == 0)
                        return "object[]";
                    
                    var elementType = GetCSharpType(array[0], $"{className}Item", settings);
                    return $"{elementType}[]";
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

        #endregion
    }
} 