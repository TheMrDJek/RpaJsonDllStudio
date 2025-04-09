using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RpaJsonDllStudio.Models;
using RpaJsonDllStudio.Services;
using System;
using System.Linq;

namespace RpaJsonDllStudio;

public class App : Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
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
            Console.WriteLine($"Ошибка при удалении DataAnnotationsValidationPlugin: {ex.Message}");
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
    }
    
    /// <summary>
    /// Получить сервис из DI контейнера
    /// </summary>
    public static T GetService<T>() where T : class
    {
        var app = Current as App;
        if (app?._serviceProvider == null)
        {
            throw new InvalidOperationException("Service provider is not initialized");
        }
        
        return app._serviceProvider.GetRequiredService<T>();
    }
}