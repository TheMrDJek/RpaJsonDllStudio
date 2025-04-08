# RpaJsonDllStudio

<div align="center">

![RpaJsonDllStudio Logo](https://via.placeholder.com/150?text=RpaJsonDllStudio)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/en-us/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia_UI-11.2.6-blue.svg)](https://avaloniaui.net/)

**Автоматическая генерация типизированных DLL из JSON для RPA-решений**

[Возможности](#возможности) • 
[Установка](#установка) • 
[Быстрый старт](#быстрый-старт) • 
[Технологии](#технологии) • 
[Разработка](#разработка)

</div>

## 📋 Содержание
- [Зачем это нужно](#зачем-это-нужно)
- [Возможности](#возможности)
- [Системные требования](#системные-требования)
- [Установка](#установка)
- [Быстрый старт](#быстрый-старт)
- [Примеры использования](#примеры-использования)
- [Технологии](#технологии)
- [Структура проекта](#структура-проекта)
- [Разработка](#разработка)
- [Вклад в проект](#вклад-в-проект)
- [Лицензия](#лицензия)

## 🎯 Зачем это нужно

В платформах RPA (UiPath, Blue Prism, Automation Anywhere) часто запрещено динамическое создание объектов во время выполнения, что усложняет работу с JSON-данными. Каждый раз приходится вручную создавать классы C# для десериализации, что отнимает время и чревато ошибками.

**RpaJsonDllStudio** решает эту проблему, автоматически превращая любую JSON-структуру в скомпилированную DLL с типизированными классами, которые можно сразу использовать в вашем RPA-проекте.

## ✨ Возможности

- **Анализ JSON структуры**:
  - Обработка вложенных объектов любой глубины
  - Поддержка массивов и коллекций
  - Автоопределение типов данных

- **Генерация типизированных C# классов**:
  - Атрибуты для корректной десериализации
  - PascalCase именование для соответствия C# стандартам
  - XML-документация для IDE-подсказок

- **Компиляция кода в DLL**:
  - Поддержка разных версий .NET
  - Оптимизация выходного кода
  - Встроенная валидация

- **Гибкость ввода**:
  - Загрузка из файлов
  - Получение из URL-источников
  - Копирование-вставка
  - Drag-and-drop файлов

- **Настраиваемость**:
  - Выбор фреймворка (.NET Standard, .NET Core, .NET Framework)
  - Библиотека JSON (Newtonsoft.Json, System.Text.Json)
  - Пользовательские namespace и именование

## 💻 Системные требования

- ОС: Windows, macOS или Linux
- Среда выполнения: .NET 9.0
- Дисковое пространство: не менее 100 МБ
- ОЗУ: 4 ГБ или более

## 📥 Установка

### Windows

1. Скачайте последнюю версию установщика из раздела [Releases](https://github.com/yourusername/RpaJsonDllStudio/releases)
2. Запустите `RpaJsonDllStudio-Setup.exe` и следуйте инструкциям мастера установки
3. После установки запустите программу из меню «Пуск» или созданного ярлыка

### macOS

1. Скачайте последнюю версию DMG-файла из раздела [Releases](https://github.com/yourusername/RpaJsonDllStudio/releases)
2. Откройте DMG-файл и перетащите приложение в папку «Программы»
3. При первом запуске может потребоваться разрешить запуск через Системные настройки > Безопасность

### Linux

```bash
# Установка через пакетный менеджер (для Debian/Ubuntu)
sudo apt install ./rpajsondllstudio_1.0.0_amd64.deb

# Или запуск AppImage
chmod +x RpaJsonDllStudio-1.0.0.AppImage
./RpaJsonDllStudio-1.0.0.AppImage
```

## 🚀 Быстрый старт

1. **Запустите RpaJsonDllStudio**
2. **Введите или загрузите JSON** в левую панель редактора
3. **Нажмите «Генерировать код»** для создания C# классов (правая панель)
4. **Настройте параметры компиляции** через меню «Компиляция» → «Настройки компиляции»
5. **Нажмите «Компилировать DLL»** и выберите путь сохранения
6. **Подключите созданную DLL** к вашему RPA-проекту и используйте сгенерированные классы

## 📋 Примеры использования

### Исходный JSON
```json
{
  "orderId": "ABC-123",
  "total": 299.99,
  "customer": {
    "id": 42,
    "name": "Иван Петров",
    "email": "ivan@example.com"
  },
  "items": [
    {
      "productId": "SKU-001",
      "quantity": 2,
      "price": 149.99
    }
  ],
  "isProcessed": false
}
```

### Получаемые C# классы
```csharp
namespace RpaJsonModels
{
    public class Root
    {
        [JsonProperty("orderId")] public string OrderId { get; set; }
        [JsonProperty("total")] public double Total { get; set; }
        [JsonProperty("customer")] public CustomerClass Customer { get; set; }
        [JsonProperty("items")] public ItemClass[] Items { get; set; }
        [JsonProperty("isProcessed")] public bool IsProcessed { get; set; }
    }

    public class CustomerClass
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
    }

    public class ItemClass
    {
        [JsonProperty("productId")] public string ProductId { get; set; }
        [JsonProperty("quantity")] public int Quantity { get; set; }
        [JsonProperty("price")] public double Price { get; set; }
    }
}
```

### Использование в RPA

```csharp
// UiPath пример
var jsonString = File.ReadAllText("data.json");
var order = JsonConvert.DeserializeObject<RpaJsonModels.Root>(jsonString);

// Теперь можно работать с типизированными данными
Console.WriteLine($"Заказ: {order.OrderId}, Клиент: {order.Customer.Name}");
foreach(var item in order.Items) {
    Console.WriteLine($"Товар: {item.ProductId}, Кол-во: {item.Quantity}");
}
```

## 🔧 Технологии

- **[.NET 9.0](https://dotnet.microsoft.com/)** - Основной фреймворк
- **[Avalonia UI](https://avaloniaui.net/)** - Кроссплатформенный UI фреймворк
- **[Roslyn](https://github.com/dotnet/roslyn)** - Компилятор C#
- **[Newtonsoft.Json](https://www.newtonsoft.com/json)** - Библиотека для работы с JSON
- **[AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)** - Редактор кода
- **[ReactiveUI](https://reactiveui.net/)** - MVVM фреймворк

## 📂 Структура проекта

```
RpaJsonDllStudio/
├── src/
│   └── RpaJsonDllStudio/          # Основной проект
│       ├── Assets/                # Ресурсы приложения
│       ├── Models/                # Модели данных
│       ├── Services/              # Сервисы для генерации кода и компиляции
│       ├── ViewModels/            # ViewModel-классы для MVVM
│       ├── Views/                 # XAML и code-behind для интерфейса
│       └── App.axaml              # Точка входа приложения
├── common/                        # Общие компоненты
├── tests/                         # Юнит и интеграционные тесты
├── RpaJsonDllStudio.sln           # Решение для Visual Studio
└── README.md                      # Этот файл
```

## 💻 Разработка

### Сборка из исходного кода

```bash
# Клонирование репозитория
git clone https://github.com/yourusername/RpaJsonDllStudio.git
cd RpaJsonDllStudio

# Установка зависимостей
dotnet restore

# Сборка проекта
dotnet build

# Запуск приложения
dotnet run --project src/RpaJsonDllStudio/RpaJsonDllStudio.csproj
```

### Требования для разработки

- .NET 9.0 SDK
- Visual Studio 2022, JetBrains Rider или VS Code

## 🤝 Вклад в проект

Мы приветствуем вклад в развитие проекта! Если вы хотите помочь:

1. Создайте fork репозитория
2. Создайте ветку для вашей функции (`git checkout -b feature/amazing-feature`)
3. Внесите изменения и сделайте коммит (`git commit -m 'Add amazing feature'`)
4. Отправьте изменения в ваш fork (`git push origin feature/amazing-feature`)
5. Откройте Pull Request в основной репозиторий

## 📄 Лицензия

Проект распространяется под [лицензией MIT](LICENSE). Используйте его свободно в личных и коммерческих проектах.

---

<div align="center">
<b>RpaJsonDllStudio</b> создан с 💙 для RPA-разработчиков

Есть вопросы или предложения? [Откройте issue](https://github.com/yourusername/RpaJsonDllStudio/issues/new)
</div>
