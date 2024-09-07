using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Core;

namespace GptChat.Views;

public partial class ChatsPanel : UserControl
{
    private Dictionary<Guid, ChatWidget> _widgets = new();

    public ChatsPanel()
    {
        InitializeComponent();
        ChatsService.Instance.Chats.CollectionChanged += ChatsOnCollectionChanged;
        ChatsService.Instance.CurrentChanged += OnCurrentChatChanged;
    }

    private void OnCurrentChatChanged(Chat? chat)
    {
        foreach (var item in _widgets.Values)
        {
            item.IsVisible = false;
        }
        Placeholder.IsVisible = chat?.Id == null;
        if (chat?.Id != null)
        {
            _widgets[chat.Id].IsVisible = true;
        }
    }

    private void ChatsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                var widget = new ChatWidget((Chat)item);
                widget.IsVisible = false;
                _widgets[widget.Chat.Id] = widget;
                Panel.Children.Add(widget);
            }
        }

        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                Panel.Children.Remove(_widgets[((Chat)item).Id]);
            }
        }
    }
}