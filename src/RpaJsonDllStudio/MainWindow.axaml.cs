using Avalonia.Controls;
using Avalonia.Input;
using RpaJsonDllStudio.ViewModels;

namespace RpaJsonDllStudio;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel = new MainWindowViewModel();

        // Подписываемся на события DragDrop для обработки перетаскивания JSON файлов
        AddHandler(DragDrop.DropEvent, Drop, handledEventsToo: true);
        AddHandler(DragDrop.DragOverEvent, DragOver, handledEventsToo: true);
        
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
        // Обновляем статус при перетаскивании файла
        _viewModel.StatusMessage = "Перетаскивание файла... Отпустите файл для загрузки.";
        
        // Разрешаем только перетаскивание файлов
        e.DragEffects = DragDropEffects.Copy;
        
        // Проверяем, содержит ли событие файлы (это предотвращает ошибки при перетаскивании не-файлов)
        if (!e.Data.Contains(DataFormats.FileNames))
        {
            e.DragEffects = DragDropEffects.None;
            _viewModel.StatusMessage = "Перетаскивать можно только файлы.";
        }
        
        // Помечаем событие как обработанное
        e.Handled = true;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        // Помечаем событие как обработанное
        e.Handled = true;
        
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
}