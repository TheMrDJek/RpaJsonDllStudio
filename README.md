# RpaJsonDllStudio

RpaJsonDllStudio - это приложение для автоматической генерации типизированных DLL из JSON файлов для использования в RPA-решениях (UiPath, Blue Prism, Automation Anywhere).

## Возможности

- Анализ JSON структуры (включая вложенные объекты и массивы)
- Генерация типизированных C# классов с атрибутами для десериализации
- Компиляция кода в готовую DLL, которую можно подключить к RPA-проекту
- Различные способы загрузки JSON: из файла, через URL, копирование-вставка, drag-and-drop
- Настройка параметров генерации и компиляции (целевой .NET, библиотека JSON, namespace и т.д.)

## Системные требования

- Windows, macOS или Linux с поддержкой .NET 9.0
- Не менее 100 МБ свободного места на диске
- 4 ГБ оперативной памяти или более

## Установка

### Windows

1. Скачайте последнюю версию из раздела [Releases](https://github.com/yourusername/RpaJsonDllStudio/releases)
2. Запустите установщик `RpaJsonDllStudio-Setup.exe` и следуйте инструкциям
3. После установки запустите RpaJsonDllStudio из меню Пуск

### macOS/Linux

1. Скачайте последнюю версию из раздела [Releases](https://github.com/yourusername/RpaJsonDllStudio/releases)
2. Распакуйте архив в любую директорию
3. Запустите исполняемый файл `RpaJsonDllStudio`

## Быстрый старт

1. Запустите RpaJsonDllStudio
2. Введите или загрузите JSON в левой панели
3. Нажмите кнопку "Генерировать код" для создания C# кода
4. Настройте параметры компиляции через меню "Компиляция" -> "Настройки компиляции"
5. Нажмите кнопку "Компилировать DLL" для создания библиотеки
6. Используйте полученную DLL в вашем RPA-проекте

## Разработка

### Сборка из исходного кода

```bash
# Клонирование репозитория
git clone https://github.com/yourusername/RpaJsonDllStudio.git
cd RpaJsonDllStudio

# Сборка
dotnet build

# Запуск
dotnet run --project src/RpaJsonDllStudio/RpaJsonDllStudio.csproj
```

### Технологии

- [.NET 9.0](https://dotnet.microsoft.com/)
- [Avalonia UI](https://avaloniaui.net/) - кроссплатформенный UI фреймворк
- [Roslyn](https://github.com/dotnet/roslyn) - компилятор C#
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON фреймворк
- [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) - редактор кода

## Лицензия

Этот проект лицензирован под [MIT License](LICENSE).
