using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RpaJsonDllStudio.Views
{
    public partial class InputDialog : Window, INotifyPropertyChanged
    {
        private string _title;
        private string _message;
        private string _value;
        private string _inputResult;
        private string _initialValue;
        private TaskCompletionSource<bool> _dialogResult;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title 
        { 
            get => _title; 
            set 
            { 
                _title = value; 
                OnPropertyChanged(); 
            } 
        }

        public string Message 
        { 
            get => _message; 
            set 
            { 
                _message = value; 
                OnPropertyChanged(); 
            } 
        }

        public string Value 
        { 
            get => _value; 
            set 
            { 
                _value = value; 
                OnPropertyChanged(); 
            } 
        }

        public string InputResult 
        { 
            get => _inputResult; 
            private set 
            { 
                _inputResult = value; 
                OnPropertyChanged(); 
            } 
        }

        public string InitialValue 
        { 
            get => _initialValue; 
            set 
            { 
                _initialValue = value; 
                OnPropertyChanged(); 
            } 
        }

        public InputDialog()
        {
            InitializeComponent();
            DataContext = this;
            _dialogResult = new TaskCompletionSource<bool>();
            _title = "Ввод";
            _message = "";
            _value = "";
            _inputResult = "";
            _initialValue = "";
        }
        
        public InputDialog(string title, string message) : this()
        {
            Title = title;
            Message = message;
            Value = "";
            InputResult = "";
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (!string.IsNullOrEmpty(InitialValue))
            {
                Value = InitialValue;
            }
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            InputResult = "";
            _dialogResult.TrySetResult(false);
            Close();
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            InputResult = Value;
            _dialogResult.TrySetResult(true);
            Close();
        }

        public new Task<bool> ShowDialog(Window parent)
        {
            if (parent != null)
            {
                base.ShowDialog(parent);
            }
            
            return _dialogResult.Task;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 