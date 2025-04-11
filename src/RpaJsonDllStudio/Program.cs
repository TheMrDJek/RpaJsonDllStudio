using Avalonia;
using Serilog;
using System;
using System.Linq;

namespace RpaJsonDllStudio;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Устанавливаем кодировку для консоли сразу при запуске программы
        // до любого вывода текста
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        bool isDevelopmentMode = false;
        
        // Проверяем, есть ли флаг отладки в аргументах или запущен ли отладчик
        if (args.Length > 0 && (args.Contains("--debug") || args.Contains("-d")))
        {
            App.EnableDebugLogging = true;
            isDevelopmentMode = true;
        }
        // Проверяем, запущено ли приложение под отладчиком (из IDE)
        else if (System.Diagnostics.Debugger.IsAttached)
        {
            App.EnableDebugLogging = true;
            isDevelopmentMode = true;
        }
        
        // Вывод информации о режиме отладки только в режиме разработки
        if (isDevelopmentMode)
        {
            Console.WriteLine("===================================");
            Console.WriteLine("  РЕЖИМ ОТЛАДКИ ВКЛЮЧЕН");
            Console.WriteLine("  Логи будут выводиться с уровнем Debug");
            Console.WriteLine("===================================");
        }
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}