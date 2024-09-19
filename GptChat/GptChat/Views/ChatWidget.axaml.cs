using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Core;
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

    public ChatWidget(Chat chat)
    {
        Chat = chat;
        InitializeComponent();
        Chat.Messages.ItemInserted += OnItemInserted;
        Chat.Messages.ItemRemoved += OnItemRemoved;
        LoadMessages();
        ApplyChanges();
        Chat.Updated += ApplyChanges;
        
        ChatSettings.GetModels();
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
            BubblesStackPanel.Children.Remove(_bubbles[obj.Id]);
            _bubbles[obj.Id].ReplyClicked -= AddReply;
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
        
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
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
        InputBox.Text = "";
        await ChatsService.Instance.CreateMessage(Chat, "user", text, ReplyList.Replys.ToList(), true);
        ReplyList.Replys.Clear();
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
            ScrollViewer.Offset = new Vector(0, ScrollViewer.ScrollBarMaximum.Y);
        _inited = true;
    }

    public void Unload()
    {
        ChatsService.Instance.UnloadMessages(Chat);
        ScrollFromBottom(0);
    }
}