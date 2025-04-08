using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using System;
using System.IO;
using System.Reflection;

namespace RpaJsonDllStudio.Views
{
    public partial class JsonEditorControl : UserControl
    {
        private TextEditor _editor;
        
        // Событие изменения текста
        public event EventHandler<string>? TextChanged;

        public JsonEditorControl()
        {
            InitializeComponent();
            
            _editor = this.Find<TextEditor>("Editor");
            
            try
            {
                // Для Avalonia.AvaloniaEdit используем встроенную подсветку синтаксиса
                var jsonSyntax = HighlightingManager.Instance.GetDefinitionByExtension(".json");
                if (jsonSyntax != null)
                {
                    _editor.SyntaxHighlighting = jsonSyntax;
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки просто продолжаем без подсветки синтаксиса
                Console.WriteLine($"Ошибка при загрузке подсветки синтаксиса: {ex.Message}");
            }
            
            // Подписываемся на событие изменения текста
            _editor.TextChanged += (s, e) => TextChanged?.Invoke(this, _editor.Text);
        }
        
        public string Text
        {
            get => _editor.Text;
            set => _editor.Text = value;
        }
    }
} 