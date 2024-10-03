using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Logging;
using Utils;
using Path = System.IO.Path;

namespace GptChat.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) {
        LogService.Init();
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            LogService.Logger.Fatal(e.Message);
            Process.Start(Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ErrorHandler"));
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}