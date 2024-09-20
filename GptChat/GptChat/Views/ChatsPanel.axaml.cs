using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Threading;
using Core;
using Core.RemoteRepository;
using GptChat.Windows;

namespace GptChat.Views;

public partial class ChatsPanel : UserControl
{
    private Dictionary<Guid, ChatWidget> _widgets = new();

    public ChatsPanel()
    {
        InitializeComponent();
        ChatsService.Instance.Chats.ItemInserted += OnItemInserted;
        ChatsService.Instance.Chats.ItemRemoved += OnItemRemoved;
        ChatsService.Instance.CurrentChanged += OnCurrentChatChanged;
        ChatSocketService.Instance.GptError += OnGptError;
        
        ChatSettings.GetModels();
    }

    private void OnGptError(string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MessageBox.Danger(text);
        });
    }

    private void OnCurrentChatChanged(Chat? chat)
    {
        foreach (var item in _widgets.Values)
        {
            if (item.IsVisible)
            {
                item.Unload();
                item.IsVisible = false;
            }
        }
        Placeholder.IsVisible = chat?.Id == null;
        if (chat?.Id != null)
        {
            _widgets[chat.Id].IsVisible = true;
        }
    }
    
    private void OnItemInserted(int index, Chat obj)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_widgets.ContainsKey(obj.Id))
                return;
            var widget = new ChatWidget(obj);
            widget.IsVisible = false;
            _widgets[widget.Chat.Id] = widget;
            Panel.Children.Insert(index, widget);
        });
    }

    private void OnItemRemoved(int index, Chat obj)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_widgets.Remove(obj.Id, out var item))
                return;
            Panel.Children.Remove(item);
        });
    }
}