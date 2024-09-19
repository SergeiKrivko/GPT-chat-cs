using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
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

        Wallpaper.CurrentChange += wallpaper => UpdatePixmap();
    }

    private void UpdatePixmap()
    {
        _timer.Enabled = true;
        _timer.Interval = 100;
    }
    
    private void OnTimer()
    {
        _timer.Enabled = false;
        Dispatcher.UIThread.Post(() => SvgViewer.Source = WallPapers(Wallpaper.Current.File, Size, _color));
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

class Wallpaper
{
    public string Name { get; }
    public string File { get; }

    private Wallpaper(string name, string file)
    {
        Name = name;
        File = file;
    }

    public static List<Wallpaper> All = new()
    {
        new Wallpaper("Спорт", "pattern-1"),
        new Wallpaper("Развлечения", "pattern-2"),
        // new Wallpaper("Спорт", "pattern-3"),
        new Wallpaper("Обои 4", "pattern-4"),
        new Wallpaper("Коты в космосе", "pattern-5"),
        new Wallpaper("Космос", "pattern-6"),
        new Wallpaper("Инструменты", "pattern-7"),
        new Wallpaper("Фастфуд", "pattern-8"),
        new Wallpaper("Магия", "pattern-9"),
        new Wallpaper("Обои 10", "pattern-10"),
        new Wallpaper("Любовь", "pattern-12"),
        new Wallpaper("Зоопарк", "pattern-13"),
        new Wallpaper("Зима", "pattern-14"),
        new Wallpaper("Космические корабли", "pattern-15"),
        new Wallpaper("Обои 16", "pattern-16"),
        new Wallpaper("Обои 17", "pattern-17"),
        new Wallpaper("Новый год", "pattern-18"),
        new Wallpaper("Подводный мир", "pattern-19"),
        new Wallpaper("Созвездия", "pattern-20"),
        new Wallpaper("Обои 21", "pattern-21"),
        new Wallpaper("Обои 22", "pattern-22"),
        new Wallpaper("Коты", "pattern-23"),
        new Wallpaper("Обои 24", "pattern-24"),
        new Wallpaper("Обои 25", "pattern-25"),
        new Wallpaper("Обои 26", "pattern-26"),
        new Wallpaper("Обои 27", "pattern-27"),
        new Wallpaper("Париж", "pattern-28"),
        new Wallpaper("Обои 29", "pattern-29"),
        new Wallpaper("Обои 30", "pattern-30"),
        new Wallpaper("Обои 31", "pattern-31"),
    };

    private static Wallpaper _current = All[10];

    public delegate void CurrentWallpaperChangeHandler(Wallpaper wallpaper);

    public static event CurrentWallpaperChangeHandler? CurrentChange;

    public static Wallpaper Current
    {
        get => _current;
        set
        {
            _current = value;
            CurrentChange?.Invoke(value);
        }
    }
}