using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace GptChat.Windows;

public partial class MessageBox : Window
{
    public enum MessageBoxType
    {
        Success,
        Warning,
        Danger,
    }
    
    public MessageBoxType Type { get; }
    
    public MessageBox(MessageBoxType type, string text)
    {
        InitializeComponent();
        Type = type;
        TextBlock.Text = text;
        switch (Type)
        {
            case MessageBoxType.Success:
                SuccessIcon.IsVisible = true;
                Title = "Успех";
                break;
            case MessageBoxType.Warning:
                WarningIcon.IsVisible = true;
                Title = "Предупреждение";
                break;
            case MessageBoxType.Danger:
                DangerIcon.IsVisible = true;
                Title = "Ошибка";
                break;
        }
    }

    private static async Task Show(MessageBoxType type, string text)
    {
        var dialog = new MessageBox(type, text);
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            await dialog.ShowDialog(desktop.MainWindow);
        }
    }

    public static async void Success(string text)
    {
        await Show(MessageBoxType.Success, text);
    }

    public static async void Warning(string text)
    {
        await Show(MessageBoxType.Warning, text);
    }

    public static async void Danger(string text)
    {
        await Show(MessageBoxType.Danger, text);
    }
}