using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace RpaJsonDllStudio.Views;

public partial class ErrorDialog : Window
{
    public ErrorDialog()
    {
        InitializeComponent();
    }

    public ErrorDialog(string title, string errorText) : this()
    {
        var titleTextBlock = this.FindControl<TextBlock>("TitleTextBlock");
        var errorTextBox = this.FindControl<TextBox>("ErrorTextBox");
        
        // Устанавливаем заголовок окна и текст ошибки
        this.Title = title;
        if (titleTextBlock != null)
            titleTextBlock.Text = title;
            
        if (errorTextBox != null)
            errorTextBox.Text = errorText;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
} 