using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System;
using System.Xml;
using System.Linq;

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
        
        // Разрешаем drag & drop для родительского элемента
        // Сам TextEditor не поддерживает напрямую AllowDrop
        this.AddHandler(DragDrop.DropEvent, EditorDrop);
        this.AddHandler(DragDrop.DragOverEvent, EditorDragOver);
    }
    
    private void EditorDragOver(object? sender, DragEventArgs e)
    {
        // Показываем, что редактор принимает только JSON и TXT файлы
        if (e.Data.Contains(DataFormats.FileNames))
        {
            var files = e.Data.GetFileNames();
            var hasValidFiles = files != null && files.Any(f => 
            {
                var ext = System.IO.Path.GetExtension(f).ToLowerInvariant();
                return ext == ".json" || ext == ".txt";
            });
            
            if (hasValidFiles)
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }
        else 
        {
            e.DragEffects = DragDropEffects.None;
        }
        
        // Не помечаем событие как обработанное, чтобы оно дошло до MainWindow
    }
    
    private void EditorDrop(object? sender, DragEventArgs e)
    {
        // Не обрабатываем событие явно, чтобы оно дошло до MainWindow
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