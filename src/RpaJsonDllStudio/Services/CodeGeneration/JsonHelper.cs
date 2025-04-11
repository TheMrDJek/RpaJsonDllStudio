using Newtonsoft.Json.Linq;
using System;

namespace RpaJsonDllStudio.Services.CodeGeneration;

/// <summary>
/// Вспомогательный класс для работы с JSON
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Проверяет валидность JSON
    /// </summary>
    /// <param name="json">JSON строка для проверки</param>
    /// <returns>True если JSON валиден, иначе False</returns>
    public static bool IsValidJson(string json)
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
} 