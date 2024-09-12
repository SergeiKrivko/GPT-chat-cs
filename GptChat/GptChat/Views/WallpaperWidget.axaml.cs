using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;

namespace GptChat.Views;

public partial class WallpaperWidget : UserControl
{
    private Size Size { get; set; }
    private const double SCALE = 3;
    private const int WIDTH = 1125;
    private const int HEIGHT = 2436;
    private Timer _timer;
    private string _color { get; set; } = "#2B5E2E";
    
    public WallpaperWidget()
    {
        InitializeComponent();
        _timer = new Timer(100);
        _timer.Elapsed += (sender, args) => OnTimer();
        _timer.AutoReset = true;
        _timer.Enabled = false;
        _timer.Start();

        var resource = Resources.GetResourceObservable("WallpaperColor");
        resource.Subscribe(o =>
        {
            _color = o?.ToString() ?? "#2B5E2E";
            UpdatePixmap();
        });
    }

    private void UpdatePixmap()
    {
        _timer.Enabled = true;
        _timer.Interval = 100;
    }
    
    private void OnTimer()
    {
        _timer.Enabled = false;
        Dispatcher.UIThread.Post(() => SvgViewer.Source = WallPapers("pattern-1", Size, _color));
    }

    private static string WallPapers(string name, Size size, string color)
    {
        color = color.Replace("#ff", "#");
        var loader = AssetLoader.Open(new Uri($"avares://GptChat/Assets/wallpapers/{name}.svg"));
        var text = new StreamReader(loader).ReadToEnd();

        var width = size.Width * SCALE;
        var height = size.Height * SCALE;

        var copiesList = new List<string>();
        for (var x = 0; x < width; x += WIDTH)
        {
            for (var y = 0; y < height; y += HEIGHT)
            {
                copiesList.Add($"<use href=\"#mainLayer\" transform=\"translate({x}, {y})\"/>");
            }
        }

        var copies = string.Join("\n    ", copiesList);

        var res =  text
            .Replace("{width}", (width / SCALE).ToString("G"))
            .Replace("{height}", (height / SCALE).ToString("G"))
            .Replace("{sized_width}", width.ToString("G"))
            .Replace("{sized_height}", height.ToString("G"))
            .Replace("{copies}", copies)
            .Replace("{color}", color);
        // Console.WriteLine(res);
        return res;
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Size = e.NewSize;
        UpdatePixmap();
    }
}