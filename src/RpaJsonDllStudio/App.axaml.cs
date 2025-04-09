using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RpaJsonDllStudio.Models;
using RpaJsonDllStudio.Services;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;

namespace RpaJsonDllStudio;

public class App : Application
{
    private IServiceProvider? _serviceProvider;
    
    // Флаг для включения детального логирования
    public static bool EnableDebugLogging { get; set; } = false;
    
    // Инициализация логгера
    private static void InitializeLogger()
    {
        // Устанавливаем кодировку консоли для корректного отображения русских символов
        // Это нужно делать всегда, независимо от режима отладки
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RpaJsonDllStudio", 
            "Logs");
            
        // Создаем директорию, если она не существует
        Directory.CreateDirectory(appDataPath);
        
        var logFilePath = Path.Combine(appDataPath, "app_.log");
        
        // Настраиваем конфигурацию логгера
        var logConfig = new LoggerConfiguration();
        
        // Устанавливаем минимальный уровень логирования в зависимости от флага отладки
        if (EnableDebugLogging)
        {
            // Если режим отладки включен, выводим все логи
            logConfig.MinimumLevel.Debug();
        }
        else
        {
            // В режиме Release выводим только важные сообщения (ошибки и критические проблемы)
            logConfig.MinimumLevel.Error();
        }
            
        // Добавляем специальный обогащающий свойствами, чтобы легче отлаживать события перетаскивания
        logConfig.Enrich.WithProperty("ApplicationVersion", "1.0.0");
        
        // Настраиваем вывод в консоль и файл
        if (EnableDebugLogging)
        {
            // В режиме отладки выводим в консоль все сообщения
            logConfig.WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Debug,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            );
        }
        else
        {
            // В режиме Release выводим в консоль только ошибки и критические проблемы
            logConfig.WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            );
        }
        
        // Настраиваем вывод в файл 
        logConfig.WriteTo.File(
            path: logFilePath,
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            encoding: System.Text.Encoding.UTF8,
            restrictedToMinimumLevel: EnableDebugLogging ? LogEventLevel.Debug : LogEventLevel.Error
        );
            
        // Создаём логгер с нашей конфигурацией
        Log.Logger = logConfig.CreateLogger();
        
        // Выводим сообщения только в режиме отладки
        if (EnableDebugLogging)
        {
            Log.Information("Приложение запущено в режиме отладки");
            Log.Debug("Операционная система: {OS}, Версия: {Version}", 
                Environment.OSVersion.Platform, 
                Environment.OSVersion.VersionString);
        }
    }

    public override void Initialize()
    {
        // Инициализируем логгер
        InitializeLogger();
        
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Для Avalonia 11.x поддержка удаления DataAnnotationsValidationPlugin
        try
        {
            // Пробуем использовать новый API, если он доступен
            var dataValidators = BindingPlugins.DataValidators;
            if (dataValidators != null)
            {
                var validatorsToRemove = dataValidators.OfType<DataAnnotationsValidationPlugin>().ToList();
                foreach (var validator in validatorsToRemove)
                {
                    dataValidators.Remove(validator);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при удалении DataAnnotationsValidationPlugin");
        }
        
        // Настраиваем DI контейнер
        ConfigureServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Регистрируем сервисы
        services.AddSingleton<ICodeGenerationService, CodeGenerationService>();
        services.AddSingleton<CompilationSettings>();
        services.AddSingleton<IBuildSettingsService, BuildSettingsService>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        if (EnableDebugLogging)
        {
            Log.Information("Сервисы зарегистрированы");
        }
    }
    
    /// <summary>
    /// Получить сервис из DI контейнера
    /// </summary>
    public static T GetService<T>() where T : class
    {
        var app = Current as App;
        if (app?._serviceProvider == null)
        {
            // Ошибка всегда логируется, независимо от режима работы
            Log.Error("Service provider не инициализирован");
            throw new InvalidOperationException("Service provider is not initialized");
        }
        
        return app._serviceProvider.GetRequiredService<T>();
    }
    
    public static void Shutdown()
    {
        if (EnableDebugLogging)
        {
            Log.Information("Приложение завершает работу");
        }
        
        // Корректно закрываем Serilog
        Log.CloseAndFlush();
        
        // Завершаем работу приложения
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}