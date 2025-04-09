using Avalonia.Controls;
using Avalonia.Platform.Storage;
using RpaJsonDllStudio.Models;
using RpaJsonDllStudio.Services;
using RpaJsonDllStudio.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Timers;
using Serilog;

namespace RpaJsonDllStudio.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly ICodeGenerationService _codeGenerationService;
    private readonly CompilationSettings _settings;
    private readonly IBuildSettingsService _buildSettingsService;
    private readonly HttpClient _httpClient;
        
    private string _jsonContent = "";
    private string _csharpContent = "";
    private string _statusMessage = "Готов к работе.";
    private JsonEditorControl? _jsonEditor;
    private CSharpEditorControl? _csharpEditor;
    private bool _isJsonEmpty = true;
    private bool _isCSharpEmpty = true;
    private List<string> _csharpErrors = new List<string>();
    private System.Timers.Timer? _validateCSharpTimer;

    #region Properties

    /// <summary>
    /// Содержимое JSON редактора
    /// </summary>
    public string JsonContent
    {
        get => _jsonContent;
        set
        {
            if (SetProperty(ref _jsonContent, value))
            {
                // Обновляем статус пустого редактора
                IsJsonEmpty = string.IsNullOrWhiteSpace(value);
                
                // При изменении JSON автоматически генерируем C# код
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _ = TryGenerateCSharpCodeAsync();
                }
            }
        }
    }

    /// <summary>
    /// Флаг, указывающий, что JSON редактор пуст
    /// </summary>
    public bool IsJsonEmpty
    {
        get => _isJsonEmpty;
        private set => SetProperty(ref _isJsonEmpty, value);
    }

    /// <summary>
    /// Содержимое C# редактора
    /// </summary>
    public string CSharpContent
    {
        get => _csharpContent;
        set
        {
            if (SetProperty(ref _csharpContent, value))
            {
                // Обновляем статус пустого редактора
                IsCSharpEmpty = string.IsNullOrWhiteSpace(value);
                
                // Запускаем отложенную проверку синтаксиса
                StartDelayedValidation();
            }
        }
    }

    /// <summary>
    /// Флаг, указывающий, что C# редактор пуст
    /// </summary>
    public bool IsCSharpEmpty
    {
        get => _isCSharpEmpty;
        private set 
        { 
            if (SetProperty(ref _isCSharpEmpty, value))
            {
                // Обновляем доступность команды компиляции
                (CompileDllCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Сообщение в статусной строке
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// JSON редактор контрол
    /// </summary>
    public Control? JsonEditor
    {
        get => _jsonEditor;
        private set => SetProperty(ref _jsonEditor, value as JsonEditorControl);
    }

    /// <summary>
    /// C# редактор контрол
    /// </summary>
    public Control? CSharpEditor
    {
        get => _csharpEditor;
        private set => SetProperty(ref _csharpEditor, value as CSharpEditorControl);
    }

    /// <summary>
    /// Список ошибок синтаксиса C# кода
    /// </summary>
    public List<string> CSharpErrors
    {
        get => _csharpErrors;
        private set => SetProperty(ref _csharpErrors, value);
    }

    #endregion

    #region Commands

    public ICommand OpenJsonFileCommand { get; }
    public ICommand LoadFromUrlCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand CopyJsonCommand { get; }
    public ICommand PasteJsonCommand { get; }
    public ICommand CopyCSharpCommand { get; }
    public ICommand GenerateCSharpCommand { get; }
    public ICommand CompileDllCommand { get; }
    public ICommand ShowSettingsCommand { get; }
    public ICommand ShowAboutCommand { get; }

    #endregion

    public MainWindowViewModel()
    {
        // Получаем сервисы через DI
        _codeGenerationService = App.GetService<ICodeGenerationService>();
        _settings = App.GetService<CompilationSettings>();
        _buildSettingsService = App.GetService<IBuildSettingsService>();
        _httpClient = new HttpClient();
            
        // Инициализируем таймер для отложенной проверки C# кода
        _validateCSharpTimer = new System.Timers.Timer(1000); // 1 секунда задержки
        _validateCSharpTimer.AutoReset = false;
        _validateCSharpTimer.Elapsed += async (sender, e) => 
        {
            // Вызываем проверку в UI-потоке
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await ValidateCSharpCodeAsync();
            });
        };
            
        // Инициализация команд
        OpenJsonFileCommand = new RelayCommand(_ => OpenJsonFileAsync());
        LoadFromUrlCommand = new RelayCommand(_ => LoadFromUrlAsync());
        ExitCommand = new RelayCommand(_ => Exit());
        CopyJsonCommand = new RelayCommand(_ => CopyJson());
        PasteJsonCommand = new RelayCommand(_ => PasteJsonAsync());
        CopyCSharpCommand = new RelayCommand(_ => CopyCSharp());
        GenerateCSharpCommand = new RelayCommand(_ => _ = TryGenerateCSharpCodeAsync());
        CompileDllCommand = new RelayCommand(
            _ => _ = CompileDllAsync(),
            _ => !IsCSharpEmpty
        );
        ShowSettingsCommand = new RelayCommand(_ => ShowSettingsAsync());
        ShowAboutCommand = new RelayCommand(_ => ShowAbout());
        
        // Инициализация редакторов
        InitializeEditors();
    }

    #region Private Methods

    /// <summary>
    /// Инициализирует редакторы
    /// </summary>
    private void InitializeEditors()
    {
        // Создаем новые редакторы
        JsonEditor = new JsonEditorControl()
        {
            Text = "",
            Width = double.NaN,
            Height = double.NaN
        };
            
        CSharpEditor = new CSharpEditorControl()
        {
            Text = "",
            Width = double.NaN,
            Height = double.NaN
        };
            
        // Привязка редакторов к контенту
        if (JsonEditor is JsonEditorControl jsonEditor)
        {
            _jsonEditor = jsonEditor;
            _jsonEditor.TextChanged += (sender, text) =>
            {
                // Очищаем HTML-сущности при изменении текста
                if (!string.IsNullOrEmpty(text))
                {
                    var cleanedText = CleanJsonFromHtmlEntities(text);
                    // Проверяем, были ли изменения после очистки
                    if (cleanedText != text)
                    {
                        // Если текст был изменен при очистке, обновим содержимое без вызова рекурсии
                        JsonContent = cleanedText;
                        
                        // Обновляем текст в редакторе только если он отличается от очищенного
                        // Это может вызвать повторное срабатывание TextChanged, но с уже очищенным текстом
                        if (_jsonEditor.Text != cleanedText)
                        {
                            _jsonEditor.Text = cleanedText;
                        }
                    }
                    else
                    {
                        // Если изменений нет, просто обновляем JsonContent
                        JsonContent = text;
                    }
                }
                else
                {
                    JsonContent = text;
                }
            };
        }
            
        if (CSharpEditor is CSharpEditorControl csharpEditor)
        {
            _csharpEditor = csharpEditor;
            _csharpEditor.TextChanged += (sender, text) =>
            {
                CSharpContent = text;
            };
        }
            
        // Инициализация флагов пустых редакторов
        IsJsonEmpty = true;
        IsCSharpEmpty = true;
            
        // Загружаем настройки билда
        _ = LoadBuildSettingsAsync();
    }
    
    /// <summary>
    /// Загружает настройки билда из файла
    /// </summary>
    private async Task LoadBuildSettingsAsync()
    {
        try
        {
            var buildSettings = await _buildSettingsService.LoadSettingsAsync();
            
            // Применяем загруженные настройки (если необходимо)
            if (!string.IsNullOrEmpty(buildSettings.BaseNamespace))
            {
                _settings.Namespace = buildSettings.BaseNamespace;
            }
            
            StatusMessage = "Настройки билда загружены";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки настроек билда: {ex.Message}";
        }
    }

    /// <summary>
    /// Сохраняет настройки билда в файл
    /// </summary>
    private async Task SaveBuildSettingsAsync()
    {
        try
        {
            // Получаем текущие настройки
            var buildSettings = _buildSettingsService.CurrentSettings;
            
            // Обновляем настройки из настроек компиляции
            buildSettings.BaseNamespace = _settings.Namespace;
            buildSettings.AddJsonAnnotations = _settings.GenerateJsonPropertyAttributes;
            
            // Сохраняем настройки
            var result = await _buildSettingsService.SaveSettingsAsync(buildSettings);
            
            if (result)
            {
                StatusMessage = "Настройки билда сохранены";
            }
            else
            {
                StatusMessage = "Ошибка сохранения настроек билда";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения настроек билда: {ex.Message}";
        }
    }

    private async Task TryGenerateCSharpCodeAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(JsonContent))
            {
                StatusMessage = "Ошибка: JSON пуст. Нечего генерировать.";
                // Очищаем C# контент если JSON пуст
                CSharpContent = string.Empty;
                return;
            }
                
            StatusMessage = "Генерация C# кода...";
                
            // Генерируем код
            var code = await _codeGenerationService.GenerateCodeFromJsonAsync(JsonContent, _settings);
                
            // Обновляем редактор C#
            if (_csharpEditor != null)
            {
                _csharpEditor.Text = code;
                CSharpContent = code;
                
                // Проверяем синтаксис сгенерированного кода
                await ValidateCSharpCodeAsync();
            }
                
            StatusMessage = "Код успешно сгенерирован";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка генерации кода: {ex.Message}";
        }
    }

    private async void OpenJsonFileAsync()
    {
        try
        {
            var window = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            var mainWindow = window?.MainWindow;
                
            if (mainWindow == null)
                return;

            var files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Открыть JSON файл",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON файлы (*.json)")
                    {
                        Patterns = new[] { "*.json" }
                    },
                    new FilePickerFileType("Все файлы (*.*)")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                var path = file.TryGetLocalPath();
                    
                if (path != null)
                {
                    LoadJsonFromFile(path);
                }
                else
                {
                    // Для неместных файлов (например, онлайн хранилище)
                    using var stream = await file.OpenReadAsync();
                    using var reader = new StreamReader(stream);
                    var json = await reader.ReadToEndAsync();
                    
                    // Очищаем JSON от HTML-сущностей
                    json = CleanJsonFromHtmlEntities(json);
                    
                    JsonContent = json;
                        
                    if (_jsonEditor != null)
                    {
                        _jsonEditor.Text = json;
                    }
                        
                    StatusMessage = $"Файл загружен: {file.Name} и очищен от HTML-сущностей";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка открытия файла: {ex.Message}";
        }
    }

    private async void LoadFromUrlAsync()
    {
        // TODO: Показать диалог для ввода URL
        var inputDialog = new InputDialog("Загрузка JSON с URL", "Введите URL JSON файла:")
        {
            InitialValue = "https://example.com/data.json"
        };
            
        var window = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
        var result = await inputDialog.ShowDialog(window?.MainWindow);
            
        if (result && !string.IsNullOrWhiteSpace(inputDialog.InputResult))
        {
            try
            {
                StatusMessage = "Загрузка JSON по URL...";
                    
                var url = inputDialog.InputResult.Trim();
                var response = await _httpClient.GetStringAsync(url);
                
                // Очищаем JSON от HTML-сущностей
                response = CleanJsonFromHtmlEntities(response);
                    
                JsonContent = response;
                    
                if (_jsonEditor != null)
                {
                    _jsonEditor.Text = response;
                }
                    
                StatusMessage = $"JSON загружен с {url} и очищен от HTML-сущностей";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки JSON: {ex.Message}";
            }
        }
    }

    private void Exit()
    {
        var window = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
        window?.Shutdown();
    }

    private void CopyJson()
    {
        if (!string.IsNullOrEmpty(JsonContent) && _jsonEditor != null)
        {
            _jsonEditor.CopyToClipboard();
            StatusMessage = "JSON скопирован в буфер обмена";
        }
    }

    private async void PasteJsonAsync()
    {
        StatusMessage = "Вставка JSON из буфера обмена...";
            
        if (_jsonEditor != null)
        {
            // Получаем текст из буфера обмена
            var clipboard = TopLevel.GetTopLevel(_jsonEditor)?
                .Clipboard;
                
            if (clipboard != null) 
            {
                try {
                    // Получаем текст из буфера обмена
                    var clipboardText = await clipboard.GetTextAsync();
                    
                    // Очищаем от HTML-сущностей, если текст не пустой
                    if (!string.IsNullOrEmpty(clipboardText)) {
                        clipboardText = CleanJsonFromHtmlEntities(clipboardText);
                        
                        // Устанавливаем текст напрямую
                        _jsonEditor.Text = clipboardText;
                        JsonContent = clipboardText;
                        StatusMessage = "JSON вставлен из буфера обмена и очищен от HTML-сущностей";
                        return;
                    }
                }
                catch (Exception ex) {
                    // Если не удалось получить текст, используем стандартный метод вставки
                    StatusMessage = $"Ошибка при обработке текста из буфера обмена: {ex.Message}";
                }
            }
            
            // Стандартный метод вставки (резервный вариант)
            _jsonEditor.PasteFromClipboard();
            // JsonContent будет обновлен через событие TextChanged
            StatusMessage = "JSON вставлен из буфера обмена";
        }
    }

    private void CopyCSharp()
    {
        if (!string.IsNullOrEmpty(CSharpContent) && _csharpEditor != null)
        {
            _csharpEditor.CopyToClipboard();
            StatusMessage = "C# код скопирован в буфер обмена";
        }
    }

    private async Task CompileDllAsync()
    {
        if (string.IsNullOrWhiteSpace(CSharpContent))
        {
            StatusMessage = "Ошибка: Нет C# кода для компиляции";
            return;
        }
            
        try
        {
            // Проверяем, задан ли путь для сохранения
            if (string.IsNullOrWhiteSpace(_settings.OutputPath))
            {
                // Если нет, просим указать путь
                var window = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                var mainWindow = window?.MainWindow;
                    
                if (mainWindow == null)
                    return;

                var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Сохранить DLL",
                    DefaultExtension = "dll",
                    SuggestedFileName = $"{_settings.Namespace}.dll",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("Dynamic Link Library (*.dll)")
                        {
                            Patterns = new[] { "*.dll" }
                        }
                    }
                });

                if (file != null)
                {
                    var path = file.TryGetLocalPath();
                    if (path != null)
                    {
                        _settings.OutputPath = path;
                    }
                    else
                    {
                        StatusMessage = "Ошибка: Невозможно получить локальный путь";
                        return;
                    }
                }
                else
                {
                    // Пользователь отменил выбор файла
                    StatusMessage = "Компиляция отменена";
                    return;
                }
            }
                
            StatusMessage = "Компиляция DLL...";
                
            try
            {
                // Компилируем код в DLL
                var result = await _codeGenerationService.CompileToDllAsync(CSharpContent, _settings.OutputPath, _settings);
                
                if (result != null)
                {
                    StatusMessage = $"DLL успешно скомпилирована: {result}";
                }
                else
                {
                    StatusMessage = "Ошибка компиляции DLL";
                }
            }
            catch (Exception ex)
            {
                // Отображаем детальную информацию об ошибке
                var errors = new List<string>();
                
                // Добавляем основное сообщение об ошибке
                errors.Add(ex.Message);
                
                // Проверяем наличие вложенного исключения
                if (ex.InnerException != null)
                {
                    errors.Add("Подробности:");
                    errors.Add(ex.InnerException.Message);
                }
                
                // Добавляем стек вызовов для отладки
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    errors.Add("Стек вызовов:");
                    errors.Add(ex.StackTrace);
                }
                
                await ShowErrorsDialogAsync("Ошибка компиляции DLL", errors);
                StatusMessage = "Ошибка компиляции DLL. Подробности в диалоговом окне.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка компиляции: {ex.Message}";
        }
    }

    private async void ShowSettingsAsync()
    {
        var window = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
        var mainWindow = window?.MainWindow;
            
        if (mainWindow == null)
            return;
            
        var settingsDialog = new SettingsDialog(_settings);
        var result = await settingsDialog.ShowDialog<bool>(mainWindow);
            
        if (result && settingsDialog.Settings != null)
        {
            // Обновляем настройки
            var settings = settingsDialog.Settings;
            _settings.TargetFramework = settings.TargetFramework;
            _settings.JsonLibrary = settings.JsonLibrary;
            _settings.Namespace = settings.Namespace;
            _settings.UsePascalCase = settings.UsePascalCase;
            _settings.GenerateProperties = settings.GenerateProperties;
            _settings.GenerateDefaultConstructor = settings.GenerateDefaultConstructor;
            _settings.GenerateJsonPropertyAttributes = settings.GenerateJsonPropertyAttributes;
            _settings.OptimizeOutput = settings.OptimizeOutput;
            _settings.GenerateXmlDocumentation = settings.GenerateXmlDocumentation;
            _settings.OutputPath = settings.OutputPath;
                
            // Если JSON не пустой, генерируем код с новыми настройками
            if (!string.IsNullOrWhiteSpace(JsonContent))
            {
                await TryGenerateCSharpCodeAsync();
            }
            
            // Сохраняем настройки билда
            await SaveBuildSettingsAsync();
                
            StatusMessage = "Настройки обновлены";
        }
    }

    private void ShowAbout()
    {
        // TODO: Показать информацию о программе
        StatusMessage = "RpaJsonDllStudio v1.0.0 - Утилита для генерации типизированных DLL из JSON";
    }

    /// <summary>
    /// Очищает текст от HTML-сущностей
    /// </summary>
    private string CleanJsonFromHtmlEntities(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;
            
        if (App.EnableDebugLogging)
        {
            Log.Debug("CleanJsonFromHtmlEntities начало, длина текста: {Length}", json.Length);
            
            // Подсчитываем количество HTML-сущностей в исходном тексте
            int nbspCount = CountOccurrences(json, "&nbsp;");
            int quotCount = CountOccurrences(json, "&quot;");
            int ampCount = CountOccurrences(json, "&amp;");
            int ltCount = CountOccurrences(json, "&lt;");
            int gtCount = CountOccurrences(json, "&gt;");
            int aposCount = CountOccurrences(json, "&apos;");
            
            Log.Debug("HTML-сущности в исходном тексте: " +
                     "nbsp={NbspCount}, quot={QuotCount}, amp={AmpCount}, " +
                     "lt={LtCount}, gt={GtCount}, apos={AposCount}",
                     nbspCount, quotCount, ampCount, ltCount, gtCount, aposCount);
            
            // Логируем первые 100 символов для анализа
            if (json.Length > 0)
            {
                string preview = json.Length > 100 ? json.Substring(0, 100) + "..." : json;
                Log.Debug("Начало текста: {Preview}", preview);
            }
        }
            
        // Замена HTML-сущностей
        var cleanedJson = json
            .Replace("&nbsp;", " ")
            .Replace("&quot;", "\"")
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&apos;", "'")
            // Можно добавить другие HTML-сущности при необходимости
            ;
        
        if (App.EnableDebugLogging && json != cleanedJson)
        {
            Log.Debug("Текст был очищен от HTML-сущностей, новая длина: {NewLength}", cleanedJson.Length);
            
            // Подсчитываем, сколько замен было сделано
            int replacements = json.Length - cleanedJson.Length;
            Log.Debug("Количество удаленных символов: {Replacements}", replacements);
        }
            
        return cleanedJson;
    }
    
    /// <summary>
    /// Подсчитывает количество вхождений подстроки в строке
    /// </summary>
    private int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        
        while ((index = text.IndexOf(pattern, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        
        return count;
    }

    /// <summary>
    /// Загружает JSON из файла
    /// </summary>
    private void LoadJsonFromFile(string fileName)
    {
        try
        {
            if (App.EnableDebugLogging)
            {
                Log.Debug("LoadJsonFromFile начало: {FileName}", fileName);
            }
            
            if (string.IsNullOrEmpty(fileName))
            {
                StatusMessage = "Ошибка: имя файла пустое";
                Log.Error("Ошибка: имя файла пустое");
                return;
            }
            
            if (!File.Exists(fileName))
            {
                StatusMessage = $"Ошибка: файл не существует: {fileName}";
                Log.Error("Ошибка: файл не существует: {FileName}", fileName);
                return;
            }
            
            // Читаем содержимое файла
            var json = File.ReadAllText(fileName);
            
            if (App.EnableDebugLogging)
            {
                Log.Debug("Файл прочитан, длина: {Length} символов", json.Length);
            }
            
            // Очищаем JSON от HTML-сущностей
            json = CleanJsonFromHtmlEntities(json);
            
            if (App.EnableDebugLogging)
            {
                Log.Debug("JSON очищен от HTML-сущностей");
            }
            
            // Устанавливаем текст в редактор
            if (_jsonEditor != null)
            {
                _jsonEditor.Text = json;
                JsonContent = json;
                
                if (App.EnableDebugLogging)
                {
                    Log.Debug("Текст установлен в редактор и JsonContent");
                }
                // IsJsonEmpty обновляется автоматически в сеттере JsonContent
            }
            else
            {
                Log.Error("Ошибка: _jsonEditor == null");
                StatusMessage = "Ошибка: редактор JSON не инициализирован";
                return;
            }
            
            StatusMessage = $"Файл загружен: {Path.GetFileName(fileName)}";
            
            if (App.EnableDebugLogging)
            {
                Log.Information("LoadJsonFromFile успешно завершен");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка в LoadJsonFromFile");
            StatusMessage = $"Ошибка загрузки файла: {ex.Message}";
        }
    }

    /// <summary>
    /// Запускает отложенную проверку C# кода
    /// </summary>
    private void StartDelayedValidation()
    {
        if (_validateCSharpTimer != null)
        {
            // Сбрасываем и перезапускаем таймер
            _validateCSharpTimer.Stop();
            _validateCSharpTimer.Start();
        }
    }
    
    /// <summary>
    /// Проверяет синтаксис C# кода
    /// </summary>
    private async Task ValidateCSharpCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(CSharpContent))
        {
            CSharpErrors = new List<string>();
            return;
        }
            
        try
        {
            // Проверяем код
            var errors = await _codeGenerationService.ValidateCSharpCodeAsync(CSharpContent, _settings);
            CSharpErrors = errors;
                
            if (errors.Count > 0)
            {
                StatusMessage = $"Найдено {errors.Count} ошибок в C# коде";
            }
            else if (!string.IsNullOrWhiteSpace(CSharpContent))
            {
                StatusMessage = "C# код корректен";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка проверки C# кода: {ex.Message}";
        }
    }
    
    private async Task ShowErrorsDialogAsync(string title, List<string> errors)
    {
        var window = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
        var mainWindow = window?.MainWindow;
            
        if (mainWindow == null)
            return;
            
        var errorText = string.Join("\n\n", errors);
        
        var dialog = new ErrorDialog(title, errorText);
        await dialog.ShowDialog(mainWindow);
    }

    /// <summary>
    /// Освобождает ресурсы
    /// </summary>
    public void Dispose()
    {
        if (_validateCSharpTimer != null)
        {
            _validateCSharpTimer.Stop();
            _validateCSharpTimer.Dispose();
            _validateCSharpTimer = null;
        }
        
        _httpClient?.Dispose();
    }

    /// <summary>
    /// Обрабатывает перетаскивание файлов в приложение
    /// </summary>
    public void HandleDroppedFiles(IEnumerable<string> fileNames)
    {
        try
        {
            if (App.EnableDebugLogging)
            {
                Log.Debug("Starting HandleDroppedFiles");
            }
            
            if (fileNames == null || !fileNames.Any())
            {
                StatusMessage = "Ошибка: список перетащенных файлов пуст.";
                Log.Error("Ошибка: список перетащенных файлов пуст.");
                return;
            }
            
            // Выводим список всех файлов для отладки
            if (App.EnableDebugLogging)
            {
                foreach (var file in fileNames)
                {
                    Log.Debug("Processing file: {FileName}", file);
                }
            }
            
            // Создаем список поддерживаемых файлов (.json и .txt) для обработки
            var supportedFiles = fileNames
                .Where(f => {
                    try {
                        var ext = Path.GetExtension(f)?.ToLowerInvariant() ?? "";
                        if (App.EnableDebugLogging)
                        {
                            Log.Debug("File {FileName}, extension: '{Extension}'", f, ext);
                        }
                        return ext == ".json" || ext == ".txt";
                    }
                    catch (Exception ex) {
                        Log.Error(ex, "Error getting extension for {FileName}", f);
                        return false;
                    }
                })
                .ToList();
            
            if (App.EnableDebugLogging)
            {
                Log.Debug("Found supported files: {Count}", supportedFiles.Count);
            }
                
            if (supportedFiles.Count == 0)
            {
                var extensions = string.Join(", ", fileNames.Select(f => {
                    try {
                        return Path.GetExtension(f)?.ToLowerInvariant() ?? "unknown";
                    }
                    catch {
                        return "error";
                    }
                }).Distinct());
                
                StatusMessage = $"Ошибка: нет поддерживаемых файлов среди перетащенных файлов. Поддерживаются только .json и .txt. Найденные расширения: {extensions}";
                Log.Error("No supported files. Extensions: {Extensions}", extensions);
                return;
            }
            
            // Берем первый поддерживаемый файл
            var selectedFile = supportedFiles[0];
            var fileName = Path.GetFileName(selectedFile);
            var fileExt = Path.GetExtension(selectedFile).ToLowerInvariant();
            
            StatusMessage = $"Загрузка файла: {fileName}...";
            
            if (App.EnableDebugLogging)
            {
                Log.Debug("Selected file for loading: {FileName}", selectedFile);
            }
            
            try
            {
                LoadJsonFromFile(selectedFile);
                
                if (fileExt == ".txt")
                {
                    StatusMessage = $"Загружен текстовый файл: {fileName}. Проверьте правильность JSON.";
                }
                else if (supportedFiles.Count > 1)
                {
                    StatusMessage = $"Загружен файл: {fileName}. Остальные {supportedFiles.Count - 1} файлов проигнорированы.";
                }
                else
                {
                    StatusMessage = $"Файл JSON успешно загружен: {fileName}";
                }
                
                if (App.EnableDebugLogging)
                {
                    Log.Information("File successfully loaded");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при загрузке файла {fileName}: {ex.Message}";
                Log.Error(ex, "Error loading file {FileName}", fileName);
                Log.Error("Stack trace: {StackTrace}", ex.StackTrace);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при обработке перетащенных файлов: {ex.Message}";
            Log.Error(ex, "General error in HandleDroppedFiles");
            Log.Error("Stack trace: {StackTrace}", ex.StackTrace);
        }
    }

    #endregion
}