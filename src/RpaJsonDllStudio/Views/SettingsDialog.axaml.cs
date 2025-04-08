using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RpaJsonDllStudio.Models;
using RpaJsonDllStudio.ViewModels;
using System;

namespace RpaJsonDllStudio.Views
{
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        public SettingsDialog(CompilationSettings settings) : this()
        {
            var viewModel = new SettingsViewModel(settings);
            viewModel.CloseRequested += (sender, result) =>
            {
                if (result)
                {
                    // Если результат True, то настройки были сохранены
                    Settings = viewModel.GetSettings();
                }
                Close(result);
            };
            
            DataContext = viewModel;
        }

        public CompilationSettings? Settings { get; private set; }
    }
} 