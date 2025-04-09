using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System;
using System.Xml;

namespace RpaJsonDllStudio.Views;

public partial class JsonEditorControl : UserControl
{
    private TextEditor _editor;
        
    // Событие изменения текста
    public event EventHandler<string>? TextChanged;

    public JsonEditorControl()
    {
        InitializeComponent();
            
        _editor = this.FindControl<TextEditor>("Editor");
            
        try
        {
            // Загружаем пользовательскую схему подсветки
            using var stream = typeof(JsonEditorControl).Assembly
                .GetManifestResourceStream("RpaJsonDllStudio.Assets.JsonSyntaxHighlighting.xshd");
                
            if (stream != null)
            {
                using var reader = new XmlTextReader(stream);
                _editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            else
            {
                // Если не удалось загрузить пользовательскую схему, используем встроенную
                var jsonSyntax = HighlightingManager.Instance.GetDefinitionByExtension(".json");
                if (jsonSyntax != null)
                {
                    _editor.SyntaxHighlighting = jsonSyntax;
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
                var jsonSyntax = HighlightingManager.Instance.GetDefinitionByExtension(".json");
                if (jsonSyntax != null)
                {
                    _editor.SyntaxHighlighting = jsonSyntax;
                }
            }
            catch
            {
                // Игнорируем ошибку
            }
        }
            
        // Подписываемся на событие изменения текста
        _editor.TextChanged += (s, e) => TextChanged?.Invoke(this, _editor.Text);
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