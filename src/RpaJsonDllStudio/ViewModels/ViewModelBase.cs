using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RpaJsonDllStudio.ViewModels;

/// <summary>
/// Базовый класс для всех ViewModels с реализацией INotifyPropertyChanged
/// </summary>
public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Метод для уведомления об изменении свойства
    /// </summary>
    /// <param name="propertyName">Имя свойства (подставляется автоматически)</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Устанавливает значение свойства и вызывает событие изменения, если значение изменилось
    /// </summary>
    /// <typeparam name="T">Тип значения</typeparam>
    /// <param name="field">Ссылка на поле</param>
    /// <param name="value">Новое значение</param>
    /// <param name="propertyName">Имя свойства (подставляется автоматически)</param>
    /// <returns>True если значение изменилось, false в противном случае</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}