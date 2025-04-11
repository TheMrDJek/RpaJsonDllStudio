using Avalonia.Controls;
using Avalonia.Input;
using RpaJsonDllStudio.ViewModels;
using System.Linq;
using System;
using Avalonia.Interactivity;
using System.IO;
using Serilog;
using System.Collections.Generic;

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
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);

        // Обработчики событий окна
        this.Opened += (s, e) => 
        {
            _viewModel.StatusMessage = "Готов к работе. Используйте меню для открытия файла или перетащите файл на приложение.";
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
            // Отладочная информация
            if (App.EnableDebugLogging)
            {
                Log.Debug("DragOver - Проверка перетаскиваемых данных");
                // Получаем доступные форматы другим способом, так как GetFormats() недоступен
                Log.Debug("Поддержка формата FileNames: {FileNames}", e.Data.Contains(DataFormats.FileNames));
            }
            
            // Проверяем, что это действительно перетаскивание файлов
            // На некоторых платформах поведение может отличаться, поэтому проверяем все варианты
            bool isFileNamesDragDrop = e.Data.Contains(DataFormats.FileNames);

            // Для ОС Windows проверяем дополнительные форматы, которые могут содержать данные о файлах
            if (!isFileNamesDragDrop)
            {
                // Проверяем известные форматы вручную
                string[] knownFormats = new[] 
                {
                    DataFormats.FileNames,
                    "FileDrop",
                    "FileGroupDescriptor",
                    "UniformResourceLocator",
                    "FilenameW"
                };
                
                foreach (var format in knownFormats)
                {
                    if (e.Data.Contains(format))
                    {
                        isFileNamesDragDrop = true;
                        if (App.EnableDebugLogging)
                        {
                            Log.Debug("Найден формат: {Format}", format);
                        }
                        break;
                    }
                }
                
                if (App.EnableDebugLogging)
                {
                    Log.Debug("Проверка дополнительных форматов: {Result}", isFileNamesDragDrop);
                }
            }
            
            // Разрешаем перетаскивание только если это файлы
            e.DragEffects = isFileNamesDragDrop ? DragDropEffects.Copy : DragDropEffects.None;
            
            // Обновляем сообщение в статусной строке
            _viewModel.StatusMessage = isFileNamesDragDrop 
                ? "Отпустите для обработки файла" 
                : "Перетаскивание не поддерживается для этого типа данных";
            
            // Дополнительно логируем информацию о файлах для отладки
            if (isFileNamesDragDrop && App.EnableDebugLogging)
            {
                try
                {
                    var files = e.Data.GetFileNames()?.ToList();
                    if (files != null && files.Any())
                    {
                        var fileInfo = string.Join(", ", files.Select(f => $"{{Path: {f}, Extension: {System.IO.Path.GetExtension(f)}}}"));
                        Log.Debug("Перетаскиваемые файлы: {Files}", fileInfo);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Не удалось получить имена файлов, но формат поддерживается");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка в DragOver");
            e.DragEffects = DragDropEffects.None;
        }
        
        // Обязательно отмечаем событие как обработанное
        e.Handled = true;
    }
    
    private void DragLeave(object? sender, DragEventArgs e)
    {
        try
        {
            // Сбрасываем состояние перетаскивания при выходе курсора за пределы области
            _viewModel.StatusMessage = "Готов к работе. Используйте меню для открытия файла или перетащите файл на приложение.";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка в DragLeave");
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        try
        {
            if (App.EnableDebugLogging)
            {
                Log.Debug("Событие Drop активировано");
                Log.Debug("Проверка наличия формата FileNames: {HasFileNames}", e.Data.Contains(DataFormats.FileNames));
                
                // Проверяем все форматы, которые поддерживаются
                string[] knownFormats = new[] 
                {
                    DataFormats.FileNames,
                    "FileDrop",
                    "FileGroupDescriptor",
                    "UniformResourceLocator",
                    "FilenameW"
                };
                
                foreach (var format in knownFormats)
                {
                    Log.Debug("Проверка формата {Format}: {Result}", format, e.Data.Contains(format));
                }
            }
            
            // Сначала проверим основной формат
            if (e.Data.Contains(DataFormats.FileNames))
            {
                HandleFileNamesDrop(e);
            }
            // Затем проверяем альтернативные форматы
            else if (e.Data.Contains("FileDrop") || e.Data.Contains("FileGroupDescriptor") || e.Data.Contains("FilenameW"))
            {
                if (App.EnableDebugLogging)
                {
                    Log.Debug("Найден альтернативный формат перетаскивания файлов");
                }
                
                // Попытка извлечь имена файлов из альтернативных форматов
                try
                {
                    // Попробуем напрямую получить имена файлов
                    var files = e.Data.GetFileNames()?.ToList();
                    if (files != null && files.Count > 0)
                    {
                        HandleFileList(files);
                    }
                    else
                    {
                        Log.Warning("Не удалось получить имена файлов из альтернативного формата");
                        _viewModel.StatusMessage = "Ошибка: не удалось получить имена перетащенных файлов.";
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при обработке альтернативного формата перетаскивания");
                    _viewModel.StatusMessage = $"Ошибка при обработке перетащенного файла: {ex.Message}";
                }
            }
            else
            {
                Log.Warning("Данные не содержат поддерживаемых форматов для перетаскивания файлов");
                _viewModel.StatusMessage = "Ошибка: перетаскиваемые данные не являются файлами.";
            }
            
            // Отмечаем событие как обработанное
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при Drop");
            _viewModel.StatusMessage = $"Ошибка при обработке перетащенного файла: {ex.Message}";
        }
    }
    
    private void HandleFileNamesDrop(DragEventArgs e)
    {
        var files = e.Data.GetFileNames()?.ToList();
        if (files != null && files.Count > 0)
        {
            if (App.EnableDebugLogging)
            {
                Log.Debug("Получено файлов: {Count}", files.Count);
                foreach (var file in files)
                {
                    Log.Debug("Файл: {File}", file);
                }
            }
            
            HandleFileList(files);
        }
        else
        {
            Log.Warning("Список файлов пуст или null");
            _viewModel.StatusMessage = "Ошибка: не удалось получить имена перетащенных файлов.";
        }
    }
    
    private void HandleFileList(List<string> files)
    {
        // Обрабатываем перетащенные файлы
        _viewModel.StatusMessage = "Файл перетащен. Обработка...";
        _viewModel.HandleDroppedFiles(files);
    }
}