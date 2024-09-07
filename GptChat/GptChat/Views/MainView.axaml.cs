using System;
using Auth;
using Avalonia.Controls;
using Core;

namespace GptChat.Views;

public partial class MainView : UserControl
{
    private readonly int _max2ColumnWidth = 500;
    private bool _twoColumn = true;
    
    public MainView()
    {
        InitializeComponent();
        ChatsService.Instance.CurrentChanged += OnCurrentChatChanged;
        AuthService.Instance.UserChanged += OnUserChanged;
        OnUserChanged(AuthService.Instance.User);
    }

    private void OnCurrentChatChanged(Chat? chat)
    {
        if (!_twoColumn)
        {
            ChatsPanel.IsVisible = ChatsService.Instance.Current != null;
            ChatsList.IsVisible = ChatsService.Instance.Current == null;
        }
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var width = e.NewSize.Width;
        if (width < _max2ColumnWidth)
        {
            _twoColumn = false;
            Border.IsVisible = false;
            ChatsList.Width = width;
            ChatsPanel.Width = width;
            ChatsPanel.IsVisible = ChatsService.Instance.Current != null;
            ChatsList.IsVisible = ChatsService.Instance.Current == null;
        }
        else
        {
            _twoColumn = true;
            ChatsPanel.IsVisible = true;
            Border.IsVisible = true;
            ChatsList.IsVisible = true;
            ChatsList.Width = int.Max(200, int.Min(300, (int)(width / 3)));
            ChatsPanel.Width = width - 1 - ChatsList.Width;
        }
    }

    private void OnUserChanged(User? user)
    {
        MainScreen.IsVisible = user != null;
        AuthScreen.IsVisible = user == null;
    }
}