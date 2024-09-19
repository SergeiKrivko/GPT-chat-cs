using System.Collections.Generic;
using Auth;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GptChat.Views;

namespace GptChat.Windows;

public partial class AppSettings : Window
{
    private List<Wallpaper> Wallpapers { get; } = Wallpaper.All;
    
    public AppSettings()
    {
        InitializeComponent();
        WallpapersBox.ItemsSource = Wallpapers;
        WallpapersBox.SelectedValue = Wallpaper.Current;
        UsernameBlock.Text = AuthService.Instance.User?.Email;
        AuthService.Instance.UserChanged += user => UsernameBlock.Text = user?.Email;
    }

    private void WallpapersBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (WallpapersBox.SelectedValue != null)
            Wallpaper.Current = (Wallpaper)WallpapersBox.SelectedValue;
    }

    private void SignOutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        AuthService.Instance.SignOut();
        Close();
    }
}