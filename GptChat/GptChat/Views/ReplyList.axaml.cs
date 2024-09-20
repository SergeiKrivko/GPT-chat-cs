using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Core;
using Core.LocalRepository;
using Utils;

namespace GptChat.Views;

public partial class ReplyList : UserControl
{
    private Dictionary<Guid, ReplyItem> _items = new();
    public ObservableList<Message> Replys { get; } = new();
    
    public ReplyList()
    {
        InitializeComponent();
        Replys.ItemInserted += OnItemInserted;
        Replys.ItemRemoved += OnItemRemoved;
        Replys.Cleared += OnCleared;
    }

    public delegate void ScrollRequestHandler(Guid messageId);

    public event ScrollRequestHandler? ScrollRequested;

    private void OnCleared()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _items.Clear();
            ReplyStackPanel.Children.Clear();
            Margin = new Thickness(0);
        });
    }

    private void OnItemInserted(int index, Message obj)
    {
        if (_items.ContainsKey(obj.Id))
            return;
        Dispatcher.UIThread.Post(() =>
        {
            var widget = new ReplyItem(obj);
            widget.RemoveClicked += m => Replys.Remove(obj);
            widget.ScrollClicked += id => ScrollRequested?.Invoke(id);
            _items[widget.Message.Id] = widget;
            ReplyStackPanel.Children.Insert(index, widget);
            Margin = new Thickness(5);
        });
    }
    
    private void OnItemRemoved(int index, Message obj)
    {
        if (!_items.ContainsKey(obj.Id))
            return;
        Dispatcher.UIThread.Post(() =>
        {
            ReplyStackPanel.Children.Remove(_items[obj.Id]);
            _items.Remove(obj.Id);
            if (Replys.Count == 0)
                Margin = new Thickness(0);
        });
    }
}