using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System;
using System.Xml;

namespace RpaJsonDllStudio.Views;

public partial class CSharpEditorControl : UserControl
{
    private TextEditor _editor;

    public CSharpEditorControl()
    {
        InitializeComponent();
            
        _editor = this.FindControl<TextEditor>("Editor");
            
        try
        {
            // Загружаем пользовательскую схему подсветки
            using var stream = typeof(CSharpEditorControl).Assembly
                .GetManifestResourceStream("RpaJsonDllStudio.Assets.CSharpSyntaxHighlighting.xshd");
                
            if (stream != null)
            {
                using var reader = new XmlTextReader(stream);
                _editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            else
            {
                // Если не удалось загрузить пользовательскую схему, используем встроенную
                var csharpSyntax = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
                if (csharpSyntax != null)
                {
                    _editor.SyntaxHighlighting = csharpSyntax;
                }
            }
        }
        catch (Exception ex)
        {
            // В случае ошибки просто продолжаем без подсветки синтаксиса
            Console.WriteLine($"Ошибка при загрузке подсветки синтаксиса: {ex.Message}");
            
            try
            {
                // Пробуем загрузить стандартную подсветку
                var csharpSyntax = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
                if (csharpSyntax != null)
                {
                    _editor.SyntaxHighlighting = csharpSyntax;
                }
            }
            catch
            {
                // Игнорируем ошибку
            }
        }
    }
        
    public string Text
    {
        get => _editor?.Text ?? string.Empty;
        set 
        {
            if (_editor != null)
                _editor.Text = value;
        }
    }
}