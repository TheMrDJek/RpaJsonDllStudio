using Avalonia.Controls;
using Avalonia.Input;
using RpaJsonDllStudio.ViewModels;
using System.Linq;
using System;
using Avalonia.Interactivity;

namespace RpaJsonDllStudio;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel = new MainWindowViewModel();

        // Добавляем обработчики событий Drag & Drop
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);

        // Обработчики событий окна
        this.Opened += (s, e) => 
        {
            _viewModel.StatusMessage = "Готов к работе. Перетащите JSON файл в окно редактора.";
        };
        
        this.Closing += (s, e) =>
        {
            // Освобождаем ресурсы при закрытии окна
            _viewModel.Dispose();
        };
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        try
        {
            _viewModel.StatusMessage = "Перетаскивание файла... Отпустите файл для загрузки.";
            
            // По умолчанию разрешаем перетаскивание
            e.DragEffects = DragDropEffects.Copy;
            
            // Проверяем, содержит ли событие файлы с поддерживаемыми расширениями
            if (e.Data.Contains(DataFormats.FileNames))
            {
                var files = e.Data.GetFileNames();
                if (files != null)
                {
                    bool hasValidFiles = files.Any(f => 
                    {
                        var ext = System.IO.Path.GetExtension(f).ToLowerInvariant();
                        return ext == ".json" || ext == ".txt";
                    });
                    
                    if (hasValidFiles)
                    {
                        e.DragEffects = DragDropEffects.Copy;
                        _viewModel.StatusMessage = "Отпустите файл JSON для загрузки...";
                    }
                    else
                    {
                        e.DragEffects = DragDropEffects.None;
                        _viewModel.StatusMessage = "Неподдерживаемый формат файла. Поддерживаются только .json и .txt.";
                    }
                }
            }
            else 
            {
                e.DragEffects = DragDropEffects.None;
                _viewModel.StatusMessage = "Перетаскивать можно только файлы.";
            }
        }
        catch (Exception ex)
        {
            // Ловим любые исключения, чтобы не допустить падения приложения
            Console.WriteLine($"Ошибка при DragOver: {ex.Message}");
            e.DragEffects = DragDropEffects.None;
            _viewModel.StatusMessage = "Ошибка при проверке перетаскиваемого файла";
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        try
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                var files = e.Data.GetFileNames();
                if (files != null)
                {
                    _viewModel.StatusMessage = "Файлы перетащены. Обработка...";
                    _viewModel.HandleDroppedFiles(files);
                }
                else
                {
                    _viewModel.StatusMessage = "Ошибка: не удалось получить имена перетащенных файлов.";
                }
            }
            else
            {
                _viewModel.StatusMessage = "Ошибка: перетащенные данные не содержат файлы.";
            }
        }
        catch (Exception ex)
        {
            // Ловим любые исключения при перетаскивании
            Console.WriteLine($"Ошибка при Drop: {ex.Message}");
            _viewModel.StatusMessage = $"Ошибка при обработке перетащенного файла: {ex.Message}";
        }
    }
}