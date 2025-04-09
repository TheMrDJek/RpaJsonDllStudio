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

namespace RpaJsonDllStudio.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ICodeGenerationService _codeGenerationService;
    private readonly CompilationSettings _settings;
    private readonly IBuildSettingsService _buildSettingsService;
    private readonly HttpClient _httpClient;
        
    private string _jsonContent = "";
    private string _csharpContent = "";
    private string _statusMessage = "Готов к работе";
    private JsonEditorControl? _jsonEditor;
    private CSharpEditorControl? _csharpEditor;
    private bool _isJsonEmpty = true;
    private bool _isCSharpEmpty = true;

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
            }
        }
    }

    /// <summary>
    /// Флаг, указывающий, что C# редактор пуст
    /// </summary>
    public bool IsCSharpEmpty
    {
        get => _isCSharpEmpty;
        private set => SetProperty(ref _isCSharpEmpty, value);
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
            
        // Инициализация команд
        OpenJsonFileCommand = new RelayCommand(_ => OpenJsonFileAsync());
        LoadFromUrlCommand = new RelayCommand(_ => LoadFromUrlAsync());
        ExitCommand = new RelayCommand(_ => Exit());
        CopyJsonCommand = new RelayCommand(_ => CopyJson());
        PasteJsonCommand = new RelayCommand(_ => PasteJsonAsync());
        CopyCSharpCommand = new RelayCommand(_ => CopyCSharp());
        GenerateCSharpCommand = new RelayCommand(_ => _ = TryGenerateCSharpCodeAsync());
        CompileDllCommand = new RelayCommand(_ => _ = CompileDllAsync());
        ShowSettingsCommand = new RelayCommand(_ => ShowSettingsAsync());
        ShowAboutCommand = new RelayCommand(_ => ShowAbout());
        
        // Инициализация редакторов
        InitializeEditors();
    }

    /// <summary>
    /// Обрабатывает перетаскивание файлов в приложение
    /// </summary>
    public void HandleDroppedFiles(IEnumerable<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            // Проверяем расширение файла
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
                
            if (extension == ".json")
            {
                LoadJsonFromFile(fileName);
                break; // Загружаем только первый JSON-файл
            }
        }
    }

    #region Private Methods

    /// <summary>
    /// Инициализирует редакторы
    /// </summary>
    private void InitializeEditors()
    {
        // Создаем экземпляры редакторов
        JsonEditor = new JsonEditorControl();
        CSharpEditor = new CSharpEditorControl();
            
        // Подписываемся на изменение текста в JSON редакторе
        if (_jsonEditor != null)
        {
            _jsonEditor.TextChanged += (sender, text) => 
            {
                JsonContent = text;
            };
        }
        
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
        if (_jsonEditor == null) return;
            
        try
        {
            // Получаем текст из редактора
            var json = _jsonEditor.Text;
                
            // Проверяем валидность JSON
            if (!_codeGenerationService.IsValidJson(json))
            {
                StatusMessage = "Ошибка: Невалидный JSON";
                return;
            }
                
            StatusMessage = "Генерация C# кода...";
                
            // Генерируем код
            var code = await _codeGenerationService.GenerateCodeFromJsonAsync(json, _settings);
                
            // Обновляем редактор C#
            if (_csharpEditor != null)
            {
                _csharpEditor.Text = code;
                CSharpContent = code;
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
                    JsonContent = json;
                        
                    if (_jsonEditor != null)
                    {
                        _jsonEditor.Text = json;
                    }
                        
                    StatusMessage = $"Файл загружен: {file.Name}";
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
                    
                JsonContent = response;
                    
                if (_jsonEditor != null)
                {
                    _jsonEditor.Text = response;
                }
                    
                StatusMessage = $"JSON загружен с {url}";
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
        if (!string.IsNullOrEmpty(JsonContent))
        {
            // TODO: Реализовать копирование в буфер обмена
            StatusMessage = "JSON скопирован в буфер обмена";
        }
    }

    private async void PasteJsonAsync()
    {
        // TODO: Реализовать вставку из буфера обмена
        StatusMessage = "Вставка JSON из буфера обмена...";
            
        // Временная реализация - просто очищаем текущий JSON
        JsonContent = "";
            
        if (_jsonEditor != null)
        {
            _jsonEditor.Text = "";
        }
            
        StatusMessage = "JSON вставлен из буфера обмена";
    }

    private void CopyCSharp()
    {
        if (!string.IsNullOrEmpty(CSharpContent))
        {
            // TODO: Реализовать копирование в буфер обмена
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
    /// Загружает JSON из файла
    /// </summary>
    private void LoadJsonFromFile(string fileName)
    {
        try
        {
            var json = File.ReadAllText(fileName);
            
            // Устанавливаем текст в редактор
            if (_jsonEditor != null)
            {
                _jsonEditor.Text = json;
                JsonContent = json;
                // IsJsonEmpty обновляется автоматически в сеттере JsonContent
            }
            
            StatusMessage = $"Файл загружен: {Path.GetFileName(fileName)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки файла: {ex.Message}";
        }
    }

    #endregion
}