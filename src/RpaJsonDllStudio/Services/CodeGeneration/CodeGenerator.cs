using Newtonsoft.Json.Linq;
using RpaJsonDllStudio.Models;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RpaJsonDllStudio.Services.CodeGeneration;

/// <summary>
/// Класс для генерации C# кода из JSON
/// </summary>
public partial class CodeGenerator
{
    // Используем генерированные регулярные выражения для оптимизации
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^\w]")]
    private static partial Regex NonWordCharRegex();

    /// <summary>
    /// Генерирует C# код из JSON строки
    /// </summary>
    public async Task<string> GenerateCodeFromJsonAsync(string json, CompilationSettings settings)
    {
        ValidateJsonInput(json);

        // Выполняем генерацию асинхронно для предотвращения блокировки UI
        return await Task.Run(() => GenerateCode(json, settings));
    }

    /// <summary>
    /// Проверяет корректность входного JSON
    /// </summary>
    private static void ValidateJsonInput(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON не может быть пустым", nameof(json));
        }

        if (JsonHelper.IsValidJson(json) == false)
        {
            throw new ArgumentException("Неверный формат JSON", nameof(json));
        }
    }

    /// <summary>
    /// Генерирует C# код из JSON строки
    /// </summary>
    private string GenerateCode(string json, CompilationSettings settings)
    {
        var rootObject = JToken.Parse(json);
        var sb = new StringBuilder();
        var rootClassBuilder = new StringBuilder();
        var otherClassesBuilder = new StringBuilder();

        AddUsingsToBuilder(sb, settings);
        AppendNamespaceStart(sb, settings.Namespace);

        GenerateClassesFromToken(rootObject, settings.RootClassName, rootClassBuilder, otherClassesBuilder,
            settings);

        sb.Append(rootClassBuilder);
        sb.Append(otherClassesBuilder);
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Добавляет начало пространства имен в StringBuilder
    /// </summary>
    private static void AppendNamespaceStart(StringBuilder sb, string namespaceName)
    {
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");
    }
    
    /// <summary>
    /// Добавляет необходимые директивы using
    /// </summary>
    private static void AddUsingsToBuilder(StringBuilder sb, CompilationSettings settings)
    {
        AppendBaseUsings(sb);
        AppendJsonLibraryUsings(sb, settings.JsonLibrary);
        sb.AppendLine();
    }

    /// <summary>
    /// Добавляет базовые директивы using
    /// </summary>
    private static void AppendBaseUsings(StringBuilder sb)
    {
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Runtime.Serialization;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine("using System.Linq;");
    }

    /// <summary>
    /// Добавляет директивы using для выбранной библиотеки JSON
    /// </summary>
    private static void AppendJsonLibraryUsings(StringBuilder sb, JsonLibrary jsonLibrary)
    {
        string[] jsonNamespaces = jsonLibrary switch
        {
            JsonLibrary.NewtonsoftJson =>
            [
                "using Newtonsoft.Json;",
                "using Newtonsoft.Json.Serialization;"
            ],

            JsonLibrary.SystemTextJson =>
            [
                "using System.Text.Json;",
                "using System.Text.Json.Serialization;"
            ],

            _ => throw new ArgumentOutOfRangeException(nameof(jsonLibrary), jsonLibrary,
                "Неподдерживаемая библиотека JSON")
        };

        foreach (var ns in jsonNamespaces)
        {
            sb.AppendLine(ns);
        }
    }

    /// <summary>
    /// Генерирует классы C# на основе JSON структуры
    /// </summary>
    private void GenerateClassesFromToken(JToken token, string className, StringBuilder? rootSb,
        StringBuilder otherClassesSb, CompilationSettings settings, int indentLevel = 1)
    {
        if (token is JObject obj)
        {
            GenerateClassFromObject(obj, className, rootSb, otherClassesSb, settings, indentLevel);
        }
        else if (token is JArray array && array.Count > 0 && array[0] is JObject firstObject)
        {
            // Если массив содержит объекты, генерируем класс для первого элемента
            GenerateClassesFromToken(firstObject, className, rootSb, otherClassesSb, settings, indentLevel);
        }
    }

    /// <summary>
    /// Генерирует класс C# из JSON объекта
    /// </summary>
    private void GenerateClassFromObject(JObject obj, string className, StringBuilder? rootSb,
        StringBuilder otherClassesSb, CompilationSettings settings, int indentLevel)
    {
        var currentClassSb = new StringBuilder();
        string indent = new string(' ', indentLevel * 4);

        AppendClassDocumentation(currentClassSb, className, indent, settings.GenerateXmlDocumentation);
        AppendClassDeclaration(currentClassSb, className, indent);

        GeneratePropertiesForObject(obj, className, currentClassSb, otherClassesSb, settings, indent, indentLevel);

        currentClassSb.AppendLine($"{indent}}}");

        // Определяем, куда добавить сгенерированный класс
        if (className == settings.RootClassName && rootSb != null)
        {
            rootSb.AppendLine(currentClassSb.ToString());
        }
        else
        {
            otherClassesSb.AppendLine(currentClassSb.ToString());
        }
    }

    /// <summary>
    /// Добавляет XML-документацию для класса
    /// </summary>
    private static void AppendClassDocumentation(StringBuilder sb, string className, string indent, bool generateXmlDocumentation)
    {
        if (generateXmlDocumentation)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Класс, представляющий {className}");
            sb.AppendLine($"{indent}/// </summary>");
        }
    }

    /// <summary>
    /// Добавляет объявление класса
    /// </summary>
    private static void AppendClassDeclaration(StringBuilder sb, string className, string indent)
    {
        sb.AppendLine($"{indent}public class {className}");
        sb.AppendLine($"{indent}{{");
    }

    /// <summary>
    /// Генерирует свойства для объекта JSON
    /// </summary>
    private void GeneratePropertiesForObject(JObject obj, string parentClassName, StringBuilder currentClassSb,
        StringBuilder otherClassesSb, CompilationSettings settings, string indent, int indentLevel)
    {
        int propertyCount = obj.Properties().ToList().Count;
        int currentPropertyIndex = 0;

        foreach (var property in obj.Properties())
        {
            currentPropertyIndex++;

            // Получаем имя и тип свойства
            var propertyInfo = GetPropertyInfo(property, parentClassName, settings);
            
            // Генерируем определение свойства
            GeneratePropertyDefinition(
                currentClassSb, 
                propertyInfo.OriginalName, 
                propertyInfo.PropertyName, 
                propertyInfo.PropertyType, 
                settings, 
                indent);

            // Добавляем пустую строку между свойствами для лучшей читабельности
            if (currentPropertyIndex < propertyCount)
            {
                currentClassSb.AppendLine();
            }

            // Генерируем дочерние классы, если необходимо
            GenerateChildClasses(
                property.Value, 
                propertyInfo.ChildClassName, 
                propertyInfo.PropertyName, 
                otherClassesSb, 
                settings, 
                indentLevel);
        }
    }

    /// <summary>
    /// Получает информацию о свойстве (имя, тип и имя дочернего класса)
    /// </summary>
    private PropertyInfo GetPropertyInfo(JProperty property, string parentClassName, CompilationSettings settings)
    {
        string originalPropertyName = property.Name;
        string cleanPropertyName = CleanupPropertyName(originalPropertyName);
        
        string propertyName = ApplyNamingConvention(cleanPropertyName, settings.UsePascalCase);
        
        // Создаем имя дочернего класса
        string childClassName = CreateChildClassName(propertyName, property.Value);
        
        // Определяем тип свойства
        string propertyType = GetCSharpType(property.Value, childClassName, settings);
        
        return new PropertyInfo(
            originalPropertyName, 
            propertyName, 
            propertyType, 
            childClassName);
    }

    /// <summary>
    /// Применяет соглашение об именовании к имени свойства
    /// </summary>
    private static string ApplyNamingConvention(string propertyName, bool usePascalCase)
    {
        if (usePascalCase && !string.IsNullOrEmpty(propertyName))
        {
            // Преобразуем первую букву в верхний регистр для PascalCase
            return char.ToUpper(propertyName[0]) + propertyName.Substring(1);
        }
        
        return propertyName;
    }

    /// <summary>
    /// Создает имя дочернего класса для свойства
    /// </summary>
    private static string CreateChildClassName(string propertyName, JToken propertyValue)
    {
        if (propertyValue is JArray && propertyName.EndsWith("s"))
        {
            // Если имя свойства заканчивается на 's', используем единственное число
            return propertyName.Substring(0, propertyName.Length - 1);
        }
        
        // Используем имя свойства как имя класса
        return propertyName;
    }

    /// <summary>
    /// Генерирует определение свойства
    /// </summary>
    private static void GeneratePropertyDefinition(
        StringBuilder sb, 
        string originalName, 
        string propertyName, 
        string propertyType, 
        CompilationSettings settings, 
        string indent)
    {
        // Добавляем атрибут JsonProperty, если нужно
        if (settings.GenerateJsonPropertyAttributes)
        {
            AddJsonPropertyAttribute(sb, originalName, settings.JsonLibrary, indent);
        }

        // Добавляем XML документацию для свойства, если нужно
        if (settings.GenerateXmlDocumentation)
        {
            AddPropertyDocumentation(sb, propertyName, indent);
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
    }

    /// <summary>
    /// Добавляет атрибут JsonProperty для свойства
    /// </summary>
    private static void AddJsonPropertyAttribute(StringBuilder sb, string originalName, JsonLibrary jsonLibrary, string indent)
    {
        if (jsonLibrary == JsonLibrary.NewtonsoftJson)
        {
            sb.AppendLine($"{indent}    [JsonProperty(\"{originalName}\")]");
        }
        else if (jsonLibrary == JsonLibrary.SystemTextJson)
        {
            sb.AppendLine($"{indent}    [JsonPropertyName(\"{originalName}\")]");
        }
    }

    /// <summary>
    /// Добавляет XML-документацию для свойства
    /// </summary>
    private static void AddPropertyDocumentation(StringBuilder sb, string propertyName, string indent)
    {
        sb.AppendLine($"{indent}    /// <summary>");
        sb.AppendLine($"{indent}    /// Свойство {propertyName}");
        sb.AppendLine($"{indent}    /// </summary>");
    }

    /// <summary>
    /// Генерирует дочерние классы для свойства, если необходимо
    /// </summary>
    private void GenerateChildClasses(
        JToken propertyValue, 
        string childClassName, 
        string propertyName, 
        StringBuilder otherClassesSb, 
        CompilationSettings settings, 
        int indentLevel)
    {
        if (propertyValue is JObject)
        {
            // Если свойство - объект, генерируем соответствующий класс
            GenerateClassesFromToken(propertyValue, childClassName, null, otherClassesSb, settings, indentLevel);
        }
        else if (propertyValue is JArray array && array.Count > 0 && array[0] is JObject)
        {
            // Для массива объектов генерируем класс для элементов массива
            string itemClassName = GetArrayItemClassName(propertyName, childClassName);
            GenerateClassesFromToken(array[0], itemClassName, null, otherClassesSb, settings, indentLevel);
        }
    }

    /// <summary>
    /// Получает имя класса для элементов массива
    /// </summary>
    private static string GetArrayItemClassName(string propertyName, string childClassName)
    {
        if (propertyName.EndsWith("s"))
        {
            // Если имя свойства заканчивается на 's', используем единственное число для типа
            return propertyName.Substring(0, propertyName.Length - 1);
        }
        
        // В других случаях добавляем суффикс Item к имени класса
        return $"{childClassName}Item";
    }

    /// <summary>
    /// Определяет C# тип для JSON значения
    /// </summary>
    private static string GetCSharpType(JToken token, string className, CompilationSettings settings)
    {
        return token.Type switch
        {
            JTokenType.Integer => "int",
            JTokenType.Float => "double",
            JTokenType.String => "string",
            JTokenType.Boolean => "bool",
            JTokenType.Date => "DateTime",
            JTokenType.Null => "object",
            JTokenType.Object => className,
            JTokenType.Array => GetArrayType(token as JArray, className, settings),
            _ => "object"
        };
    }

    /// <summary>
    /// Определяет C# тип для массива
    /// </summary>
    private static string GetArrayType(JArray array, string className, CompilationSettings settings)
    {
        if (array.Count == 0)
        {
            return "object[]";
        }

        if (array[0] is JObject)
        {
            // Используем имя класса как есть для типа элементов массива
            return $"{className}[]";
        }
        else
        {
            var elementType = GetCSharpType(array[0], className, settings);
            return $"{elementType}[]";
        }
    }

    /// <summary>
    /// Очищает строку от HTML-сущностей и заменяет лишние пробелы символом подчеркивания
    /// </summary>
    private static string CleanupPropertyName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "EmptyProperty";
        }

        // Заменяем HTML неразрывные пробелы на символ подчеркивания
        string result = name.Replace("&nbsp;", "_");

        // Заменяем последовательности обычных пробелов на одиночный символ подчеркивания
        result = WhitespaceRegex().Replace(result, "_");

        // Удаляем все недопустимые символы для имен C# (оставляем только буквы, цифры и подчеркивания)
        result = NonWordCharRegex().Replace(result, "_");

        // Если имя начинается с цифры, добавляем префикс
        if (char.IsDigit(result[0]))
        {
            result = "Prop_" + result;
        }

        return result;
    }

    /// <summary>
    /// Вспомогательный класс для хранения информации о свойстве
    /// </summary>
    private class PropertyInfo
    {
        public string OriginalName { get; }
        public string PropertyName { get; }
        public string PropertyType { get; }
        public string ChildClassName { get; }

        public PropertyInfo(string originalName, string propertyName, string propertyType, string childClassName)
        {
            OriginalName = originalName;
            PropertyName = propertyName;
            PropertyType = propertyType;
            ChildClassName = childClassName;
        }
    }
} 