﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Core;
using Core.RemoteRepository;
using Core.Search;
using GptChat.Windows;

namespace GptChat.Views;

public partial class ChatWidget : UserControl
{
    public Chat Chat { get; }
    private Dictionary<Guid, Bubble> _bubbles = new();
    private bool _toBottom = true;
    private double _offsetFromBottom;
    private bool _loading;
    private bool _inited;
    private List<SearchResult> _searchResults = new();
    private int _currentSearchIndex = 0;

    public ChatWidget(Chat chat)
    {
        Chat = chat;
        InitializeComponent();
        Chat.Messages.ItemInserted += OnItemInserted;
        Chat.Messages.ItemRemoved += OnItemRemoved;
        LoadMessages();
        ApplyChanges();
        Chat.Updated += ApplyChanges;
        ChatSocketService.Instance.MessageFinished += message =>
        {
            if (message.ChatId == Chat.Id)
                Dispatcher.UIThread.Post(() => GptWritingBlock.IsVisible = false);
        };
        InputBox.Text = "";
    }

    private void ApplyChanges()
    {
        Dispatcher.UIThread.Post(() => { ChatNameTextBlock.Text = Chat.Name; });
    }

    private async void LoadMessages()
    {
        if (_loading)
            return;
        _loading = true;
        await ChatsService.Instance.LoadMessages(Chat.Id, 5);
        await Task.Delay(500);
        _loading = false;
    }

    private void OnItemInserted(int index, Message obj)
    {
        if (_bubbles.ContainsKey(obj.Id))
            return;
        Dispatcher.UIThread.Post(() =>
        {
            var widget = new Bubble(obj);
            widget.ReplyClicked += AddReply;
            widget.ScrollRequested += ScrollToMessage;
            _bubbles[widget.Message.Id] = widget;
            try
            {
                BubblesStackPanel.Children.Insert(index, widget);
                if (index == 0 || _toBottom)
                {
                    ScrollFromBottom(_offsetFromBottom);
                }
                else
                {
                    ScrollFromTop(ScrollViewer.Offset.Y);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        });
    }

    private void AddReply(Message message)
    {
        if (ReplyList.Replys.Contains(message))
            return;
        ReplyList.Replys.Add(message);
    }

    private void OnItemRemoved(int index, Message obj)
    {
        if (!_bubbles.ContainsKey(obj.Id))
            return;
        Dispatcher.UIThread.Post(() =>
        {
            if (!_bubbles.ContainsKey(obj.Id))
                return;
            BubblesStackPanel.Children.Remove(_bubbles[obj.Id]);
            _bubbles[obj.Id].ReplyClicked -= AddReply;
            _bubbles[obj.Id].ScrollRequested -= ScrollToMessage;
            _bubbles.Remove(obj.Id);
        });
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ChatsService.Instance.Current = null;
    }

    private async void SettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new ChatSettings();

        dialog.Chat = Chat;
        dialog.Update();

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow != null)
        {
            await dialog.ShowDialog(desktop.MainWindow);
        }
    }

    private void SendButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Send();
    }

    private async void Send()
    {
        var text = InputBox.Text;
        if (String.IsNullOrEmpty(text))
            return;
        await ChatsService.Instance.CreateMessage(Chat, "user", text, ReplyList.Replys.ToList(), true);
        InputBox.Text = "";
        ReplyList.Replys.Clear();
        GptWritingBlock.IsVisible = true;
    }

    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (!_loading)
        {
            _offsetFromBottom = ScrollViewer.ScrollBarMaximum.Y - ScrollViewer.Offset.Y;
            _toBottom = _offsetFromBottom <= 20;
            DownButton.IsVisible = _offsetFromBottom > 100;
        }

        if (ScrollViewer.Offset.Y < 100)
        {
            LoadMessages();
        }
    }

    private async void ScrollFromTop(double offset)
    {
        await Task.Delay(10);
        ScrollViewer.Offset = new Vector(0, offset);
    }

    private async void ScrollFromBottom(double offset)
    {
        await Task.Delay(10);
        ScrollViewer.Offset = new Vector(0, ScrollViewer.ScrollBarMaximum.Y - offset);
    }

    private void ScrollToMessage(Guid messageId)
    {
        if (_bubbles.TryGetValue(messageId, out var targetBubble))
        {
            ScrollFromTop(targetBubble.Bounds.Top);
        }
    }

    private void DownButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ScrollViewer.Offset = new Vector(0, ScrollViewer.ScrollBarMaximum.Y);
    }

    private void InputBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
            {
                InputBox.Text = InputBox.Text?.Insert(InputBox.CaretIndex, "\n");
                InputBox.CaretIndex += 1;
            }
            else
            {
                Send();
            }
        }
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (!_inited)
        {
            ScrollViewer.Offset = new Vector(0, ScrollViewer.ScrollBarMaximum.Y);
            _inited = true;
        }

        foreach (var bubble in _bubbles.Values)
        {
            bubble.MaxBubbleWidth = e.NewSize.Width * 0.7;
        }
    }

    public void Unload()
    {
        ChatsService.Instance.UnloadMessages(Chat);
        ScrollFromBottom(0);
    }

    private async void InputBox_OnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            var text = await clipboard.GetTextAsync() ?? "";
            InputBox.Text = InputBox.Text?.Insert(InputBox.CaretIndex, text);
            InputBox.CaretIndex += text.Length;
        }
    }

    private void SearchButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        SearchPanel.IsVisible = SearchButton.IsChecked ?? false;
    }


    private async void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchResults = await Searcher.Search(Chat.Id, SearchBox.Text);
        _currentSearchIndex = _searchResults.Count - 1;
        if (_searchResults.Count > 0)
        {
            _searchResults[^1].Selected = true;
        }
        SearchCountBlock.Text = $"{_currentSearchIndex + 1} / {_searchResults.Count}";
        
        foreach (var bubble in _bubbles.Values)
        {
            bubble.SearchResults.Clear();
        }

        foreach (var result in _searchResults)
        {
            if (_bubbles.TryGetValue(result.MessageId, out var bubble))
                bubble.SearchResults.Add(result);
        }
        foreach (var bubble in _bubbles.Values)
        {
            bubble.Update();
        }
    }

    private void SelectSearchResult(int index)
    {
        _searchResults[_currentSearchIndex].Selected = false;
        if (_searchResults[_currentSearchIndex].MessageId != _searchResults[index].MessageId)
        {
            _bubbles[_searchResults[_currentSearchIndex].MessageId].Update();
        }

        _currentSearchIndex = index;
        _searchResults[index].Selected = true;
        _bubbles[_searchResults[index].MessageId].Update();
        SearchCountBlock.Text = $"{_currentSearchIndex + 1} / {_searchResults.Count}";
        
        ScrollToMessage(_searchResults[index].MessageId);
    }

    private void ButtonNext_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_currentSearchIndex < _searchResults.Count - 1)
        {
            SelectSearchResult(_currentSearchIndex + 1);
        }
    }

    private void ButtonPrevious_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_currentSearchIndex > 0)
        {
            SelectSearchResult(_currentSearchIndex - 1);
        }
    }
}