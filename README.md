# RpaJsonDllStudio

<div align="center">

![RpaJsonDllStudio Logo](src/RpaJsonDllStudio/Assets/app-icon.ico)

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
  - Возможность задать имя корневого класса

- **Компиляция кода в DLL**:
  - Поддержка разных версий .NET
  - Оптимизация выходного кода
  - Встроенная валидация

- **Гибкость ввода**:
  - Загрузка из файлов
  - Получение из URL-источников
  - Копирование-вставка
  - Drag-and-drop файлов JSON непосредственно в редактор

- **Настраиваемость**:
  - Выбор фреймворка (.NET Standard, .NET Core, .NET Framework)
  - Библиотека JSON (Newtonsoft.Json, System.Text.Json)
  - Пользовательские namespace и именование классов
  - Настройка имени корневого класса для лучшей интеграции

- **Удобный пользовательский интерфейс**:
  - Визуальные подсказки для пустых редакторов (Drag & Drop индикаторы)
  - Редакторы кода с подсветкой синтаксиса и нумерацией строк
  - Настраиваемый интерфейс с разделителем между панелями
  - Регулируемый размер панели с ошибками компиляции
  - Высокая контрастность для улучшенной читаемости кода
  - Оптимизированные размеры окна для комфортной работы с JSON файлами

## 💻 Системные требования

- ОС: Windows, macOS или Linux
- Среда выполнения: .NET 9.0
- Для компиляции DLL:
  - Для .NET Framework 4.8 DLL: требуется установленный .NET Framework 4.8 на Windows
  - Для .NET Standard 2.0/2.1 DLL: отдельных требований нет (совместимость обеспечивается платформой приложения)
- Дисковое пространство: не менее 100 МБ
- ОЗУ: 4 ГБ или более
- Рекомендуемое разрешение экрана: от 1280×800

## Требования к SDK

Для компиляции DLL в определенный целевой фреймворк:

- **.NET Framework 4.8**: Требуется установленный .NET Framework 4.8 на Windows
- **.NET Standard 2.0/2.1**: Специальных требований нет. Возможность компиляции обеспечивается самим приложением

Приложение автоматически проверяет наличие .NET Framework 4.8 при выборе этой платформы и уведомит вас, если он не установлен.

Вы можете загрузить .NET Framework 4.8 с [официального сайта Microsoft](https://dotnet.microsoft.com/download/dotnet-framework).

## 📥 Установка

### Windows

В работе

### macOS

В работе

### Linux

В работе

## 🚀 Быстрый старт

1. **Запустите RpaJsonDllStudio**
   - При запуске вы увидите интерфейс с двумя панелями: для JSON (слева) и C# кода (справа)
   - Визуальные индикаторы подсказывают, что нужно сделать с пустыми панелями

2. **Введите или загрузите JSON** в левую панель редактора:
   - Откройте JSON файл через меню "Файл" → "Открыть JSON..."
   - Загрузите JSON с URL через меню "Файл" → "Загрузить из URL..."
   - Вставьте JSON из буфера обмена ("Правка" → "Вставить JSON")
   - Просто перетащите JSON файл из проводника в окно приложения

3. **Нажмите «Генерировать код»** для создания C# классов (правая панель)

4. **Настройте параметры компиляции** через меню «Компиляция» → «Настройки компиляции»
   - При необходимости задайте имя корневого класса (по умолчанию "Root")
   - Выберите целевой фреймворк (.NET) в соответствии с вашими потребностями
   - Убедитесь, что выбранный SDK установлен на вашем компьютере

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
    public class Order // Вместо стандартного "Root" можно задать более подходящее имя
    {
        [JsonProperty("orderId")] public string OrderId { get; set; }
        [JsonProperty("total")] public double Total { get; set; }
        [JsonProperty("customer")] public OrderCustomerClass Customer { get; set; }
        [JsonProperty("items")] public OrderItemsClass[] Items { get; set; }
        [JsonProperty("isProcessed")] public bool IsProcessed { get; set; }
    }

    public class OrderCustomerClass
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
    }

    public class OrderItemsClass
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
var order = JsonConvert.DeserializeObject<RpaJsonModels.Order>(jsonString); // Используем заданное имя класса

// Теперь можно работать с типизированными данными
Console.WriteLine($"Заказ: {order.OrderId}, Клиент: {order.Customer.Name}");
foreach(var item in order.Items) {
    Console.WriteLine($"Товар: {item.ProductId}, Кол-во: {item.Quantity}");
}
```

### Интерфейс приложения

![Интерфейс RpaJsonDllStudio](/common/interface.png)

Интерфейс приложения оптимизирован для комфортной работы:
- Окно имеет размер 1200×800 пикселей, что позволяет видеть весь JSON файл средней длины
- Панель JSON позволяет легко загружать файлы через drag & drop
- Редакторы кода оснащены подсветкой синтаксиса и номерами строк
- Панели можно изменять в размере с помощью разделителя
- Панель с ошибками компиляции имеет регулируемую высоту, что позволяет просмотреть детально все ошибки
- Визуальные подсказки в пустых панелях редактора

### Функция Drag & Drop

Приложение поддерживает перетаскивание файлов:
- Перетащите JSON файл прямо в окно приложения
- Визуальный индикатор показывает, куда нужно перетащить файл
- При загрузке файла автоматически генерируется C# код

### Настройки компиляции

![Настройки компиляции](/common/interfaceSettings.png)

В диалоге настроек компиляции вы можете указать:
- Целевой фреймворк (.NET Standard, .NET Core и т.д.)
  - **Важно**: для компиляции DLL под выбранный фреймворк соответствующий SDK должен быть установлен на вашем компьютере
  - Приложение проверит наличие SDK и предупредит, если нужный SDK не установлен
- Библиотеку JSON для работы (Newtonsoft.Json или System.Text.Json)
- Пространство имен для классов
- **Имя корневого класса** — для лучшей читаемости и интеграции с вашим проектом
- Различные опции форматирования и оптимизации кода

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
│       │   ├── AppResources.axaml # Основные ресурсы (цвета, размеры, шрифты)
│       │   ├── AppStyles.axaml    # Стили элементов интерфейса
│       │   ├── DropFileIcon.png   # Иконка перетаскивания файлов
│       │   └── app-icon.ico       # Иконка приложения
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
git clone https://github.com/themrdjek/RpaJsonDllStudio.git
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

Есть вопросы или предложения? [Откройте issue](https://github.com/themrdjek/RpaJsonDllStudio/issues/new)
</div>
