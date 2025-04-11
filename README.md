# RpaJsonDllStudio

<div align="center">

![RpaJsonDllStudio Logo](src/RpaJsonDllStudio/Assets/app-icon.ico)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/en-us/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia_UI-11.2.1-blue.svg)](https://avaloniaui.net/)

**Автоматическая генерация типизированных DLL из JSON для PIX RPA, Primo и UIPath**

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

В платформах RPA, таких как PIX RPA, Primo и UIPath, нельзя использовать динамическое создание объектов во время выполнения, что существенно усложняет работу с JSON-данными. Единственные доступные способы работы с JSON в этих платформах — это использование специальных активностей по типу "Парсинг JSON" или "Получить свойство JSON", которые преобразуют строку в `System.Text.Json.JsonElement`, с которым приходится работать через Xpath и обращения к элементам.

Это создает следующие проблемы:
- Неудобный доступ к вложенным данным
- Объекты типа `System.Text.Json.JsonElement` не отображаются корректно в отладке
- Сложные JSON-структуры требуют отдельной отладки в IDE
- Невозможность использовать автодополнение и типизацию
- Повышенный риск ошибок при обращении к свойствам JSON

**RpaJsonDllStudio** решает эти проблемы, автоматически превращая любую JSON-структуру в скомпилированную DLL с типизированными классами, которые можно сразу использовать в вашем RPA-проекте. С типизированными классами вы получаете:

- Удобный доступ к данным через свойства объектов
- Полноценное отображение в отладчике
- Поддержку автодополнения в среде разработки
- Безопасную типизацию и проверку на этапе компиляции
- Значительное ускорение разработки и уменьшение количества ошибок

## ✨ Возможности

- **Анализ JSON структуры**:
  - Обработка вложенных объектов любой глубины
  - Поддержка массивов и коллекций
  - Автоопределение типов данных
  - Совместимость с форматами JSON, которые используются в PIX RPA, Primo и UIPath

- **Генерация типизированных C# классов**:
  - Атрибуты для корректной десериализации
  - PascalCase именование для соответствия C# стандартам
  - XML-документация для IDE-подсказок
  - Возможность задать имя корневого класса
  - Оптимизировано для интеграции с RPA-платформами

- **Компиляция кода в DLL**:
  - Поддержка .NET Framework 4.8 (для UIPath)
  - Поддержка .NET Standard 2.0 (для PIX RPA и Primo)
  - Оптимизация выходного кода
  - Встроенная валидация с понятными сообщениями об ошибках

- **Гибкость ввода**:
  - Загрузка из файлов
  - Получение из URL-источников (API, веб-сервисы)
  - Копирование-вставка JSON из буфера обмена
  - Drag-and-drop файлов JSON непосредственно в редактор

- **Автоматическая очистка JSON**:
  - Удаление HTML-сущностей (`&nbsp;`, `&quot;`, `&amp;`, и др.) при генерации C# кода
  - Устранение проблем с пробелами и специальными символами без модификации исходного JSON
  - Подготовка JSON-данных, полученных из веб-источников, для создания корректных C# классов
  - Предотвращение ошибок парсинга из-за неправильно отформатированного JSON

- **Гибкие настройки**:
  - Выбор фреймворка под конкретную RPA-платформу
  - Библиотека JSON (Newtonsoft.Json, System.Text.Json)
  - Пользовательские namespace и именование классов
  - Настройка имени корневого класса для лучшей интеграции с вашей моделью данных

- **Удобный пользовательский интерфейс**:
  - Визуальные подсказки для пустых редакторов
  - Редакторы кода с подсветкой синтаксиса и нумерацией строк
  - Настраиваемый интерфейс с разделителем между панелями
  - Регулируемый размер панели с ошибками компиляции
  - Высокая контрастность для улучшенной читаемости кода
  - Оптимизированные размеры окна для комфортной работы с JSON файлами

- **Производительность и надежность**:
  - Оптимизированные логи (вывод только в режиме отладки)
  - Правильная обработка кодировки текста (UTF-8)
  - Стабильная работа с файлами любого размера
  - Уменьшенный размер исполняемого файла

## 💻 Системные требования

- ОС: Windows, macOS или Linux
- Среда выполнения: .NET 9.0
- Для компиляции DLL:
  - Для .NET Framework 4.8 DLL: требуется установленный .NET Framework 4.8 на Windows
  - Для .NET Standard 2.0/2.1 DLL: отдельных требований нет (совместимость обеспечивается платформой приложения)
- Дисковое пространство: не менее 100 МБ
- ОЗУ:
  - Для самого приложения: ~100 МБ
  - Рекомендуемые требования к системе: 2 ГБ или более (для комфортной работы с ОС)
- Рекомендуемое разрешение экрана: от 1280×800

> **Примечание:** Само приложение потребляет не более 100 МБ оперативной памяти, но для комфортной работы с IDE и RPA-платформами рекомендуется иметь не менее 2 ГБ ОЗУ в системе. При работе со сложными JSON-структурами или при частой компиляции потребление может незначительно увеличиваться.

## Требования к SDK

Для компиляции DLL в определенный целевой фреймворк:

- **.NET Framework 4.8**: Требуется установленный .NET Framework 4.8 на Windows
- **.NET Standard 2.0/2.1**: Специальных требований нет. Возможность компиляции обеспечивается самим приложением

Приложение автоматически проверяет наличие .NET Framework 4.8 при выборе этой платформы и уведомит вас, если он не установлен.

Вы можете загрузить .NET Framework 4.8 с [официального сайта Microsoft](https://dotnet.microsoft.com/download/dotnet-framework).

## 📥 Установка

### Windows

1. Скачайте последний релиз установщика для Windows из раздела [Releases](https://github.com/themrdjek/RpaJsonDllStudio/releases)
2. Запустите скачанный файл установщика `RpaJsonDllStudio_Windows_Setup_v1.0.0.exe`
3. Следуйте инструкциям мастера установки
4. После завершения установки приложение будет доступно в меню "Пуск"

**Примечание**: Установщик автоматически проверяет наличие .NET 9.0 (Runtime или SDK) и предложит скачать его, если он не установлен. Можно пропустить эту проверку, запустив установщик с параметром `/SKIPDOTNETCHECK`.

### macOS и Linux

Установщики для macOS и Linux находятся в разработке. 

**Временное решение**: Вы можете запустить приложение на этих платформах, загрузив исходный код и собрав его вручную согласно инструкциям в разделе [Разработка](#разработка).

### Технические детали

Для создания Windows-инсталлятора используется [Inno Setup](https://jrsoftware.org/isinfo.php) - бесплатный инструмент для создания профессиональных установщиков для Windows. Установщик включает:
- Проверку системных требований (Windows 10 или новее)
- Проверку наличия .NET 9.0 (Runtime или SDK)
- Создание ярлыков в меню "Пуск" и на рабочем столе (опционально)
- Возможность запуска приложения сразу после установки

## 🚀 Быстрый старт

1. **Запустите RpaJsonDllStudio**
   - При запуске вы увидите интерфейс с двумя панелями: для JSON (слева) и C# кода (справа)
   - Визуальные индикаторы подсказывают, что нужно сделать с пустыми панелями

2. **Введите или загрузите JSON** в левую панель редактора:
   - Откройте JSON файл через меню "Файл" → "Открыть JSON..."
   - Загрузите JSON с URL через меню "Файл" → "Загрузить из URL..." (например, из API вашей системы)
   - Вставьте JSON из буфера обмена ("Правка" → "Вставить JSON")
   - Просто перетащите JSON файл из проводника в окно приложения

3. **Нажмите «Генерировать код»** для создания C# классов (правая панель)
   - Проверьте сгенерированный код - он должен соответствовать структуре вашего JSON
   - Обратите внимание на имена классов и их свойства

4. **Настройте параметры компиляции** через меню «Компиляция» → «Настройки компиляции»
   - Выберите целевую платформу:
     - Для PIX RPA: .NET Standard 2.0
     - Для Primo: .NET Standard 2.0 или .NET Framework 4.8
     - Для UIPath: .NET Framework 4.8
   - Задайте имя корневого класса (по умолчанию "Root") - например, "Order", "Customer" и т.д.
   - Укажите namespace (по умолчанию "RpaJsonModels")
   - Выберите библиотеку JSON (рекомендуется Newtonsoft.Json, в PIX используется System.Text.Json) 

5. **Нажмите «Компилировать DLL»** и выберите путь сохранения

6. **Подключите созданную DLL к вашему RPA-проекту**:
   - **В PIX RPA**: добавьте DLL через "Ссылки" → "Добавить ссылку"
   - **В Primo**: импортируйте DLL через "Библиотеки" → "Добавить библиотеку"
   - **В UIPath**: добавьте DLL через "Управление пакетами" → "Добавить локальный пакет"

7. **Используйте сгенерированные классы в своем проекте**:
   ```csharp
   // Десериализация JSON в типизированный объект
   var jsonData = File.ReadAllText("data.json");
   var typedObject = JsonConvert.DeserializeObject<RpaJsonModels.YourRootClass>(jsonData);
   
   // Теперь у вас есть доступ к полям через свойства объекта
   Console.WriteLine(typedObject.SomeProperty);
   ```

## 📋 Примеры использования

### Исходный JSON
```json
{
  "id": 12345,
  "title": "Компактный пример JSON",
  "isActive": true,
  "price": 199.99,
  "inStock": null,
  "createdAt": "2023-12-25T10:15:30Z",
  "lastModified": "2023-12-26",
  "tags": ["sample", "demo", "test"],
  "emptyArray": [],
  "numbers": [1, 2, 3.5, -10, 0],
  "config": {
    "enabled": true,
    "maxRetries": 3,
    "timeout": 30.5,
    "options": {
      "logging": false,
      "cacheSize": 1024,
      "encoding": "UTF-8"
    }
  },
  "contact": {
    "name": {
      "first": "Иван",
      "last": "Петров"
    },
    "details": {
      "email": "ivan@example.ru",
      "phone": "+7 (999) 123-45-67"
    }
  },
  "items": [
    {
      "id": "item-1",
      "name": "Товар 1",
      "quantity": 5,
      "properties": {
        "color": "красный",
        "size": "M"
      }
    },
    {
      "id": "item-2",
      "name": "Товар 2",
      "quantity": 10,
      "properties": {
        "color": "синий",
        "weight": 2.5,
        "dimensions": {
          "width": 20,
          "height": 15,
          "depth": 10
        }
      }
    }
  ],
  "specialChars": "Кавычки: \", Табуляции: \t, Переносы строк: \n",
  "nestedArrays": [
    [1, 2],
    [3, 4, 5],
    ["a", "b", "c"]
  ],
  "mixedTypes": [42, "строка", true, null, { "key": "value" }]
}
```

### Получаемые C# классы
```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RpaJsonModels
{
    public class SampleData // Задаем имя корневого класса вместо стандартного "Root"
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("isActive")] public bool IsActive { get; set; }
        [JsonPropertyName("price")] public double Price { get; set; }
        [JsonPropertyName("inStock")] public object InStock { get; set; }
        [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonPropertyName("lastModified")] public string LastModified { get; set; }
        [JsonPropertyName("tags")] public string[] Tags { get; set; }
        [JsonPropertyName("emptyArray")] public object[] EmptyArray { get; set; }
        [JsonPropertyName("numbers")] public double[] Numbers { get; set; }
        [JsonPropertyName("config")] public Config Config { get; set; }
        [JsonPropertyName("contact")] public Contact Contact { get; set; }
        [JsonPropertyName("items")] public Item[] Items { get; set; }
        [JsonPropertyName("specialChars")] public string SpecialChars { get; set; }
        [JsonPropertyName("nestedArrays")] public object[][] NestedArrays { get; set; }
        [JsonPropertyName("mixedTypes")] public object[] MixedTypes { get; set; }
    }

    public class Config
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; }
        [JsonPropertyName("maxRetries")] public int MaxRetries { get; set; }
        [JsonPropertyName("timeout")] public double Timeout { get; set; }
        [JsonPropertyName("options")] public Options Options { get; set; }
    }

    public class Options
    {
        [JsonPropertyName("logging")] public bool Logging { get; set; }
        [JsonPropertyName("cacheSize")] public int CacheSize { get; set; }
        [JsonPropertyName("encoding")] public string Encoding { get; set; }
    }

    public class Contact
    {
        [JsonPropertyName("name")] public Name Name { get; set; }
        [JsonPropertyName("details")] public Details Details { get; set; }
    }

    public class Name
    {
        [JsonPropertyName("first")] public string First { get; set; }
        [JsonPropertyName("last")] public string Last { get; set; }
    }

    public class Details
    {
        [JsonPropertyName("email")] public string Email { get; set; }
        [JsonPropertyName("phone")] public string Phone { get; set; }
    }

    public class Item
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("quantity")] public int Quantity { get; set; }
        [JsonPropertyName("properties")] public Properties Properties { get; set; }
    }

    public class Properties
    {
        [JsonPropertyName("color")] public string Color { get; set; }
        [JsonPropertyName("size")] public string Size { get; set; }
        [JsonPropertyName("weight")] public double? Weight { get; set; }
        [JsonPropertyName("dimensions")] public Dimensions Dimensions { get; set; }
    }

    public class Dimensions
    {
        [JsonPropertyName("width")] public int Width { get; set; }
        [JsonPropertyName("height")] public int Height { get; set; }
        [JsonPropertyName("depth")] public int Depth { get; set; }
    }
}
```

### Использование в RPA

#### Чтение JSON в проектах RPA

##### Нетипизированный подход (неудобный способ):
```csharp
// Пример для UIPath, PIX RPA или Primo
var jsonString = File.ReadAllText("invoice.json");
var jsonDocument = JsonDocument.Parse(jsonString);
var jsonObject = jsonDocument.RootElement;

// Сложный и неудобный доступ к данным через строковые ключи
var invoiceNumber = jsonObject.GetProperty("invoiceNumber").GetString();
var customerName = jsonObject.GetProperty("customer").GetProperty("companyName").GetString();
var contactEmail = jsonObject.GetProperty("customer").GetProperty("contactPerson").GetProperty("email").GetString();

// Работа с массивами особенно сложна
var items = jsonObject.GetProperty("items");
decimal totalBeforeTax = 0;

// Приходится вручную обрабатывать каждый элемент
foreach (var item in items.EnumerateArray())
{
    // Преобразования типов могут вызвать ошибки в рантайме
    totalBeforeTax += item.GetProperty("subtotal").GetDecimal();
    
    // Доступ по индексу требует дополнительных проверок
    if (item.TryGetProperty("description", out var descriptionProp))
    {
        var description = descriptionProp.GetString();
        // обработка...
    }
}

// Отображение всей структуры в логах почти невозможно
// Ошибки доступа к несуществующим свойствам обнаруживаются только при выполнении
try 
{
    var nonExistentField = jsonObject.GetProperty("someField").GetProperty("nestedField").GetString();
} 
catch (Exception ex) 
{
    // JsonException в рантайме
}

// Важно: не забыть освободить ресурсы
jsonDocument.Dispose();
```

##### Типизированный подход с использованием RpaJsonDllStudio:
```csharp
// Пример для UIPath, PIX RPA или Primo
var jsonString = File.ReadAllText("invoice.json");
// Один вызов для получения полностью типизированного объекта
var invoice = JsonSerializer.Deserialize<RpaJsonModels.Invoice>(jsonString);

// Прямой доступ к свойствам с проверкой типов
string invoiceNumber = invoice.InvoiceNumber;
string customerName = invoice.Customer.CompanyName;
string contactEmail = invoice.Customer.ContactPerson.Email;

// Удобная работа с коллекциями
decimal totalBeforeTax = 0;
foreach (var item in invoice.Items)
{
    // Типы данных уже приведены к нужным
    totalBeforeTax += (decimal)item.Subtotal;
    
    // Автодополнение помогает найти нужные свойства
    string description = item.Description;
    
    // Полноценная строгая типизация
    if (item.TaxRate > 0)
    {
        // Работа с типизированными данными
    }
}

// Вся структура доступна в логах и отладчике
// Компилятор сразу обнаружит ошибки доступа к несуществующим свойствам
// invoice.NonExistentProperty; // Ошибка на этапе компиляции, а не в рантайме
```

#### Создание и запись JSON в проектах RPA

##### Нетипизированный подход (неудобный способ):
```csharp
// Создание сложного JSON объекта без типизации
var jsonString = @"{
  ""invoiceNumber"": ""INV-2023-05-19"",
  ""dateIssued"": """ + DateTime.Now.ToString("o") + @""",
  ""dueDate"": """ + DateTime.Now.AddDays(30).ToString("yyyy-MM-dd") + @""",
  ""totalAmount"": 2150.80,
  ""currency"": ""RUB"",
  ""status"": ""draft"",
  ""customer"": {
    ""id"": 1043,
    ""companyName"": ""АО Инновации"",
    ""contactPerson"": {
      ""firstName"": ""Мария"",
      ""lastName"": ""Петрова"",
      ""position"": ""Генеральный директор"",
      ""email"": ""m.petrova@innovations.ru"",
      ""phone"": ""+7 (495) 987-65-43""
    },
    ""billingAddress"": {
      ""street"": ""пр-т Ленина, 45"",
      ""city"": ""Санкт-Петербург"",
      ""postalCode"": ""190000"",
      ""country"": ""Россия""
    },
    ""vatNumber"": ""RU12345678"",
    ""isVip"": true
  },
  ""items"": [
    {
      ""id"": ""PROD-002"",
      ""description"": ""Лицензия на ПО RPA Professional"",
      ""quantity"": 5,
      ""unitPrice"": 250.00,
      ""subtotal"": 1250.00,
      ""taxRate"": 20,
      ""taxAmount"": 250.00
    },
    {
      ""id"": ""SERV-003"",
      ""description"": ""Обучение персонала"",
      ""quantity"": 10,
      ""unitPrice"": 65.00,
      ""subtotal"": 650.00,
      ""taxRate"": 0,
      ""taxAmount"": 0
    }
  ]
}";

// Парсинг JSON для проверки корректности
var jsonDocument = JsonDocument.Parse(jsonString);
var jsonObject = jsonDocument.RootElement;

// Запись в файл
File.WriteAllText("new_invoice.json", jsonString);

// Освобождение ресурсов
jsonDocument.Dispose();

// Минусы:
// - Большой объем кода, трудно поддерживать
// - Высокая вероятность опечаток в именах свойств
// - Нет автодополнения в среде RPA
// - Необходимость ручного отслеживания структуры
// - Сложность создания сложных объектов
```

##### Типизированный подход с использованием RpaJsonDllStudio:
```csharp
// Создание сложного JSON объекта с типизированными классами
var invoice = new RpaJsonModels.Invoice
{
    InvoiceNumber = "INV-2023-05-19",
    DateIssued = DateTime.Now,
    DueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
    TotalAmount = 2150.80,
    Currency = "RUB",
    Status = "draft",
    
    // Структурированное создание вложенных объектов
    Customer = new RpaJsonModels.Customer
    {
        Id = 1043,
        CompanyName = "АО Инновации",
        ContactPerson = new RpaJsonModels.ContactPerson
        {
            FirstName = "Мария",
            LastName = "Петрова",
            Position = "Генеральный директор",
            Email = "m.petrova@innovations.ru",
            Phone = "+7 (495) 987-65-43"
        },
        BillingAddress = new RpaJsonModels.BillingAddress
        {
            Street = "пр-т Ленина, 45",
            City = "Санкт-Петербург",
            PostalCode = "190000",
            Country = "Россия"
        },
        VatNumber = "RU12345678",
        IsVip = true
    },
    
    // Простое создание массивов с типизированными элементами
    Items = new[]
    {
        new RpaJsonModels.Item
        {
            Id = "PROD-002",
            Description = "Лицензия на ПО RPA Professional",
            Quantity = 5,
            UnitPrice = 250.00,
            Subtotal = 1250.00,
            TaxRate = 20,
            TaxAmount = 250.00
        },
        new RpaJsonModels.Item
        {
            Id = "SERV-003",
            Description = "Обучение персонала",
            Quantity = 10,
            UnitPrice = 65.00,
            Subtotal = 650.00,
            TaxRate = 0,
            TaxAmount = 0
        }
    },
    
    PaymentDetails = new RpaJsonModels.PaymentDetails
    {
        Method = "bankTransfer",
        BankAccount = new RpaJsonModels.BankAccount
        {
            AccountNumber = "40702810123450001235",
            BankName = "ВТБ",
            SwiftCode = "VTBRRUMM"
        },
        PaymentDue = new RpaJsonModels.PaymentDue
        {
            Amount = 2150.80,
            Currency = "RUB"
        }
    },
    
    Metadata = new RpaJsonModels.Metadata
    {
        CreatedBy = "robot",
        Department = "sales",
        Tags = new[] { "corporate", "training", "software" },
        Notes = null
    }
};

// Одна строка для сериализации в JSON
var jsonString = JsonSerializer.Serialize(invoice, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText("new_invoice.json", jsonString);

// Преимущества:
// - Структурированный и компактный код
// - Автодополнение свойств в среде RPA
// - Проверка типов на этапе компиляции
// - Гарантия соответствия структуры документа
```

### Интерфейс приложения

![Интерфейс RpaJsonDllStudio](common/interfaceMain.png)

Интерфейс приложения оптимизирован для комфортной работы:
- Окно имеет размер 1200×800 пикселей, что позволяет видеть весь JSON файл средней длины
- Панель JSON позволяет легко загружать файлы через drag & drop
- Редакторы кода оснащены подсветкой синтаксиса и номерами строк
- Панели можно изменять в размере с помощью разделителя
- Панель с ошибками компиляции имеет регулируемую высоту, что позволяет просмотреть детально все ошибки
- Визуальные подсказки в пустых панелях редактора

#### Успешная генерация C# кода

![Корректный C# код](common/intrfaceValid.png)

При успешной загрузке JSON и генерации кода:
- Статусная строка показывает, что "C# код корректен"
- Сгенерированный код полностью соответствует структуре JSON
- Кнопка "Компилировать DLL" становится активной
- Все классы и свойства правильно типизированы

#### Обнаружение ошибок в коде

![Ошибки в C# коде](common/interfaceNotValid.png)

Приложение автоматически проверяет сгенерированный C# код:
- При обнаружении ошибок показывается панель с детальным списком проблем
- Каждая ошибка содержит номер строки и описание проблемы
- Статусная строка показывает количество найденных ошибок
- Кнопка "Компилировать DLL" становится недоступной до исправления ошибок
- В примере показаны типичные ошибки синтаксиса C#, которые блокируют компиляцию

### Функция Drag & Drop

Приложение поддерживает перетаскивание файлов:
- Перетащите JSON файл прямо в окно приложения
- При загрузке файла автоматически генерируется C# код
- Поддерживаются форматы JSON (.json) и TXT (.txt)

### Очистка HTML-сущностей

RpaJsonDllStudio автоматически очищает загруженный JSON от HTML-сущностей при генерации C# кода, сохраняя исходный JSON без изменений:
- Замена `&nbsp;` на обычные пробелы
- Замена `&quot;` на кавычки (`"`)
- Замена `&amp;` на амперсанд (`&`)
- Замена `&lt;` и `&gt;` на < и > соответственно
- Замена `&apos;` на апостроф (`'`)

Приложение использует два разных метода очистки HTML-сущностей:
1. **Очистка при загрузке данных**: Метод `CleanJsonFromHtmlEntities` применяется для обработки данных при загрузке из файла, URL или буфера обмена. Этот метод сохраняет структуру JSON и преобразует только HTML-сущности.
2. **Очистка при генерации имен свойств**: Метод `CleanupPropertyName` используется во время генерации C# классов для создания корректных имен свойств без недопустимых символов, заменяя HTML-сущности и другие проблемные символы.

Это особенно полезно при работе с JSON, полученным из веб-источников, где такие замены часто приводят к проблемам при парсинге. Важно отметить, что исходный JSON в левой панели редактора остается без изменений, очистка применяется только к данным, используемым для генерации C# классов.

### Настройки компиляции

![Настройки компиляции](/common/interfaceSettings.png)

В диалоге настроек компиляции вы можете указать:
- Целевой фреймворк (.NET Standard, .NET Core и т.д.)
  - **Для PIX RPA**: рекомендуется .NET Standard 2.0
  - **Для Primo**: рекомендуется .NET Standard 2.0 или .NET Framework 4.8
  - **Для UIPath**: поддерживается .NET Framework 4.8
  - **Важно**: для компиляции DLL под выбранный фреймворк соответствующий SDK должен быть установлен на вашем компьютере
  - Приложение проверит наличие SDK и предупредит, если нужный SDK не установлен
- Библиотеку JSON для работы:
  - **Newtonsoft.Json** (рекомендуется для всех платформ)
  - **System.Text.Json** (для новых версий PIX RPA и UIPath)
- Пространство имен для классов (по умолчанию: RpaJsonModels)
- **Имя корневого класса** — для лучшей читаемости и интеграции с вашим проектом
- Различные опции форматирования и оптимизации кода

### Режим отладки

Приложение поддерживает запуск в режиме отладки:
- Запустите с параметром командной строки `--debug` или `-d`
- Логи будут выводиться с более детальной информацией
- Полезно при разработке или отладке сложных JSON-структур

В обычном режиме (без флага отладки) приложение работает более оптимально и выводит только критические сообщения.

### Удобный интерфейс работы с ошибками

RpaJsonDllStudio обеспечивает удобную работу с ошибками компиляции:

- **Регулируемый размер панели с ошибками**: Вы можете изменять высоту панели с ошибками с помощью разделителя, появляющегося при наличии ошибок. Это особенно полезно при работе со сложными JSON-структурами, когда компилятор может выдавать множество ошибок.

- **Детальная информация**: Каждая ошибка содержит информацию о строке кода, характере проблемы и возможных способах её устранения.

- **Мгновенная валидация**: C# код автоматически проверяется на наличие ошибок в реальном времени, что позволяет быстро выявлять и исправлять проблемы.

- **Интеграция с рабочим процессом**: Кнопка компиляции DLL автоматически отключается, если в коде присутствуют ошибки, предотвращая создание некорректных библиотек.

Эти возможности особенно важны для RPA-разработчиков, которым необходимо быстро создавать и интегрировать типизированные модели данных в свои проекты PIX RPA, Primo и UIPath.

## 🔧 Технологии

- **[.NET 9.0](https://dotnet.microsoft.com/)** - Основной фреймворк
- **[Avalonia UI](https://avaloniaui.net/)** - Кроссплатформенный UI фреймворк
- **[Roslyn](https://github.com/dotnet/roslyn)** - Компилятор C#
- **[Newtonsoft.Json](https://www.newtonsoft.com/json)** - Библиотека для работы с JSON
- **[AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)** - Редактор кода
- **[ReactiveUI](https://reactiveui.net/)** - MVVM фреймворк
- **[Serilog](https://serilog.net/)** - Структурированное логирование

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
│       ├── Converters/            # Конвертеры для XAML привязок
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

# Запуск приложения в режиме отладки
dotnet run --project src/RpaJsonDllStudio/RpaJsonDllStudio.csproj -- --debug
```

### Создание установщика

Для Windows используется Inno Setup для создания профессионального установщика:

```bash
# 1. Сначала соберите приложение для Windows
dotnet publish src/RpaJsonDllStudio/RpaJsonDllStudio.csproj -c Release -r win-x64 --self-contained

# 2. Скомпилируйте установщик с помощью Inno Setup
# Установите Inno Setup: https://jrsoftware.org/isdl.php
# Откройте файл installers/windows/setup_windows.iss в Inno Setup Compiler
# и нажмите Build -> Compile или F9
```

Исходный код установщика находится в папке `installers/windows/` и может быть адаптирован под ваши нужды.

Установщики для macOS и Linux находятся в разработке и будут доступны в будущих релизах.

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
