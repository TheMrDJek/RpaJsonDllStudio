using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RpaJsonDllStudio.ViewModels;
using System;

namespace RpaJsonDllStudio;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel = new MainWindowViewModel();

        // Подписываемся на события DragDrop для обработки перетаскивания JSON файлов
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        // Разрешаем только перетаскивание файлов
        e.DragEffects = e.DragEffects & DragDropEffects.Copy;
        
        // Проверяем, содержит ли событие файлы (это предотвращает ошибки при перетаскивании не-файлов)
        if (!e.Data.Contains(DataFormats.FileNames))
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.FileNames))
        {
            var files = e.Data.GetFileNames();
            if (files != null)
            {
                _viewModel.HandleDroppedFiles(files);
            }
        }
    }
}