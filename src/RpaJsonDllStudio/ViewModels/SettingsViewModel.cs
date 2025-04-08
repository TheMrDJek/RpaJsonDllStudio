using Avalonia.Controls;
using Avalonia.Platform.Storage;
using RpaJsonDllStudio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace RpaJsonDllStudio.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private List<string> _targetFrameworks;
        private List<string> _jsonLibraries;
        private string _selectedTargetFramework;
        private string _selectedJsonLibrary;
        private string _namespace;
        private bool _usePascalCase;
        private bool _generateProperties;
        private bool _generateDefaultConstructor;
        private bool _generateJsonPropertyAttributes;
        private bool _optimizeOutput;
        private bool _generateXmlDocumentation;
        private string _outputPath;

        public event EventHandler<bool>? CloseRequested;

        public ICommand SaveCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand BrowseCommand { get; }

        public SettingsViewModel()
        {
            // Инициализация списков
            _targetFrameworks = new List<string>
            {
                "netstandard2.0",
                "netstandard2.1",
                "net48",
                "net6.0",
                "net7.0",
                "net8.0",
                "net9.0"
            };

            _jsonLibraries = new List<string>
            {
                "Newtonsoft.Json",
                "System.Text.Json"
            };

            // Установка значений по умолчанию
            _selectedTargetFramework = _targetFrameworks[0]; // netstandard2.0
            _selectedJsonLibrary = _jsonLibraries[0]; // Newtonsoft.Json
            _namespace = "RpaJsonModels";
            _usePascalCase = true;
            _generateProperties = true;
            _generateDefaultConstructor = true;
            _generateJsonPropertyAttributes = true;
            _optimizeOutput = true;
            _generateXmlDocumentation = false;
            _outputPath = "";

            // Инициализация команд
            SaveCommand = new RelayCommand(_ => Save());
            ResetToDefaultsCommand = new RelayCommand(_ => ResetToDefaults());
            BrowseCommand = new RelayCommand(_ => BrowseOutputPath());
        }

        public SettingsViewModel(CompilationSettings settings) : this()
        {
            // Установка значений из настроек
            _selectedTargetFramework = settings.GetTargetFrameworkString();
            _selectedJsonLibrary = settings.JsonLibrary == JsonLibrary.NewtonsoftJson ? "Newtonsoft.Json" : "System.Text.Json";
            _namespace = settings.Namespace;
            _usePascalCase = settings.UsePascalCase;
            _generateProperties = settings.GenerateProperties;
            _generateDefaultConstructor = settings.GenerateDefaultConstructor;
            _generateJsonPropertyAttributes = settings.GenerateJsonPropertyAttributes;
            _optimizeOutput = settings.OptimizeOutput;
            _generateXmlDocumentation = settings.GenerateXmlDocumentation;
            _outputPath = settings.OutputPath;
        }

        public List<string> TargetFrameworks => _targetFrameworks;

        public List<string> JsonLibraries => _jsonLibraries;

        public string SelectedTargetFramework
        {
            get => _selectedTargetFramework;
            set => SetProperty(ref _selectedTargetFramework, value);
        }

        public string SelectedJsonLibrary
        {
            get => _selectedJsonLibrary;
            set => SetProperty(ref _selectedJsonLibrary, value);
        }

        public string Namespace
        {
            get => _namespace;
            set => SetProperty(ref _namespace, value);
        }

        public bool UsePascalCase
        {
            get => _usePascalCase;
            set => SetProperty(ref _usePascalCase, value);
        }

        public bool GenerateProperties
        {
            get => _generateProperties;
            set => SetProperty(ref _generateProperties, value);
        }

        public bool GenerateDefaultConstructor
        {
            get => _generateDefaultConstructor;
            set => SetProperty(ref _generateDefaultConstructor, value);
        }

        public bool GenerateJsonPropertyAttributes
        {
            get => _generateJsonPropertyAttributes;
            set => SetProperty(ref _generateJsonPropertyAttributes, value);
        }

        public bool OptimizeOutput
        {
            get => _optimizeOutput;
            set => SetProperty(ref _optimizeOutput, value);
        }

        public bool GenerateXmlDocumentation
        {
            get => _generateXmlDocumentation;
            set => SetProperty(ref _generateXmlDocumentation, value);
        }

        public string OutputPath
        {
            get => _outputPath;
            set => SetProperty(ref _outputPath, value);
        }

        public CompilationSettings GetSettings()
        {
            return new CompilationSettings
            {
                TargetFramework = GetTargetFrameworkEnum(),
                JsonLibrary = _selectedJsonLibrary == "Newtonsoft.Json" ? JsonLibrary.NewtonsoftJson : JsonLibrary.SystemTextJson,
                Namespace = _namespace,
                UsePascalCase = _usePascalCase,
                GenerateProperties = _generateProperties,
                GenerateDefaultConstructor = _generateDefaultConstructor,
                GenerateJsonPropertyAttributes = _generateJsonPropertyAttributes,
                OptimizeOutput = _optimizeOutput,
                GenerateXmlDocumentation = _generateXmlDocumentation,
                OutputPath = _outputPath
            };
        }

        private TargetFramework GetTargetFrameworkEnum()
        {
            return _selectedTargetFramework switch
            {
                "netstandard2.0" => TargetFramework.NetStandard20,
                "netstandard2.1" => TargetFramework.NetStandard21,
                "net48" => TargetFramework.NetFramework48,
                "net6.0" => TargetFramework.Net60,
                "net7.0" => TargetFramework.Net70,
                "net8.0" => TargetFramework.Net80,
                "net9.0" => TargetFramework.Net90,
                _ => TargetFramework.NetStandard20
            };
        }

        private void Save()
        {
            // Закрываем диалог с результатом True
            CloseRequested?.Invoke(this, true);
        }

        private void ResetToDefaults()
        {
            // Сбрасываем настройки на значения по умолчанию
            SelectedTargetFramework = _targetFrameworks[0]; // netstandard2.0
            SelectedJsonLibrary = _jsonLibraries[0]; // Newtonsoft.Json
            Namespace = "RpaJsonModels";
            UsePascalCase = true;
            GenerateProperties = true;
            GenerateDefaultConstructor = true;
            GenerateJsonPropertyAttributes = true;
            OptimizeOutput = true;
            GenerateXmlDocumentation = false;
            OutputPath = "";
        }

        private async void BrowseOutputPath()
        {
            var window = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            var mainWindow = window?.MainWindow;
            
            if (mainWindow == null)
                return;

            var saveFileDialog = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Выберите путь сохранения DLL",
                DefaultExtension = "dll",
                SuggestedFileName = "RpaJsonModels.dll",
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Dynamic Link Library (*.dll)")
                    {
                        Patterns = new[] { "*.dll" }
                    }
                }
            });

            if (saveFileDialog != null)
            {
                OutputPath = saveFileDialog.TryGetLocalPath() ?? "";
            }
        }
    }
} 