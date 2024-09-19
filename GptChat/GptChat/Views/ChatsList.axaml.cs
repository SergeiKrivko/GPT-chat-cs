using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Core;
using GptChat.Windows;

namespace GptChat.Views;

public partial class ChatsList : UserControl
{
    public ChatsService Service { get; } = ChatsService.Instance;
    private Dictionary<Guid, ChatsListItem> _items = new ();
    
    public ChatsList()
    {
        InitializeComponent();
        // ChatsListBox.ItemsSource = Service.Chats;
        ChatsService.Instance.CurrentChanged += OnCurrentChatChanged;
        ChatsService.Instance.Chats.ItemInserted += OnItemInserted;
        ChatsService.Instance.Chats.ItemRemoved += OnItemRemoved;
        ChatsService.Instance.Chats.ItemMoved += OnItemMoved;
    }

    private void OnItemInserted(int index, Chat obj)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_items.ContainsKey(obj.Id))
                return;
            var widget = new ChatsListItem(obj);
            widget.Selected += chat => ChatsService.Instance.Current = chat;
            _items[widget.Chat.Id] = widget;
            ChatsPanel.Children.Insert(index, widget);
        });
    }

    private void OnItemRemoved(int index, Chat obj)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_items.Remove(obj.Id, out var item))
                return;
            ChatsPanel.Children.Remove(item);
        });
    }

    private void OnItemMoved(Chat obj, int index)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_items.TryGetValue(obj.Id, out var item))
                return;
            ChatsPanel.Children.Remove(item);
            ChatsPanel.Children.Insert(index, item);
        });
    }

    private void OnCurrentChatChanged(Chat? currentChat)
    {
        foreach (var chat in ChatsService.Instance.Chats)
        {
            _items[chat.Id].IsSelected = false;
        }
        if (currentChat != null)
            _items[currentChat.Id].IsSelected = true;
    }

    private async void CreateChatButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ChatsService.Instance.CreateChat();
    }

    private async void SettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new AppSettings();
        
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            await dialog.ShowDialog(desktop.MainWindow);
        }
    }
}