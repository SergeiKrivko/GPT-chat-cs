using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Utils;

namespace ErrorHandler;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Load();
    }

    private async Task<string> ReadLogFile()
    {
        while (true)
        {
            try
            {
                return await File.ReadAllTextAsync(LogService.LogFilePath);
            }
            catch (IOException)
            {
                await Task.Delay(200);
            }
        }
    }

    private void Load()
    {
        LogBlock.Text = File.ReadAllText(LogService.LogFilePath);
    }

    private async void ButtonSend_OnClick(object? sender, RoutedEventArgs e)
    {
        var httpService = new LogHttpService();
        await httpService.SendLog(await ReadLogFile());
        Close();
    }

    private void ButtonNotSend_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}