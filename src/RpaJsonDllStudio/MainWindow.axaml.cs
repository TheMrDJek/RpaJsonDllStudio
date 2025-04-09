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

        // Обработчики событий окна
        this.Opened += (s, e) => 
        {
            _viewModel.StatusMessage = "Готов к работе. Используйте меню для открытия файла.";
        };
        
        this.Closing += (s, e) =>
        {
            // Освобождаем ресурсы при закрытии окна
            _viewModel.Dispose();
        };
    }
}