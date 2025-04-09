using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace RpaJsonDllStudio.Converters
{
    /// <summary>
    /// Конвертер из bool в Brush
    /// </summary>
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool condition = (bool)value;
            
            if (parameter is string paramString)
            {
                var parts = paramString.Split('|');
                if (parts.Length == 2)
                {
                    string trueValue = parts[0];
                    string falseValue = parts[1];
                    
                    string selectedValue = condition ? trueValue : falseValue;
                    
                    // Поддержка ссылок на ресурсы
                    if (selectedValue.StartsWith("{StaticResource ") && selectedValue.EndsWith("}"))
                    {
                        var resourceKey = selectedValue.Substring(16, selectedValue.Length - 17);
                        return Application.Current?.Resources[resourceKey];
                    }
                    
                    // Попытка преобразования в цвет
                    try
                    {
                        if (selectedValue.StartsWith("#"))
                        {
                            return new SolidColorBrush(Color.Parse(selectedValue));
                        }
                        return new SolidColorBrush(Color.Parse(selectedValue));
                    }
                    catch
                    {
                        return new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
            
            // Значение по умолчанию
            return condition 
                ? new SolidColorBrush(Color.Parse("#3F3F3F3F")) 
                : new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 