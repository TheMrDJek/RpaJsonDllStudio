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
    public partial class CSharpEditorControl : UserControl
    {
        private TextEditor _editor;

        public CSharpEditorControl()
        {
            InitializeComponent();
            
            _editor = this.Find<TextEditor>("Editor");
            
            try
            {
                // Для Avalonia.AvaloniaEdit используем встроенную подсветку синтаксиса
                var csharpSyntax = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
                if (csharpSyntax != null)
                {
                    _editor.SyntaxHighlighting = csharpSyntax;
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки просто продолжаем без подсветки синтаксиса
                Console.WriteLine($"Ошибка при загрузке подсветки синтаксиса: {ex.Message}");
            }
        }
        
        public string Text
        {
            get => _editor.Text;
            set => _editor.Text = value;
        }
    }
} 