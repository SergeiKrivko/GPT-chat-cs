using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Core;

namespace GptChat.Views;

public partial class ChatWidget : UserControl
{
    public Chat Chat { get; }
    private Dictionary<Guid, Bubble> _bubbles = new();
    
    public ChatWidget(Chat chat)
    {
        Chat = chat;
        InitializeComponent();
        ChatSettingsView.IsVisible = false;
        ChatSettingsView.Chat = Chat;
        ChatSettingsView.Update();
        Chat.Messages.ItemInserted += OnItemInserted;
        Chat.Messages.ItemRemoved += OnItemRemoved;
        LoadMessages();
        ApplyChanges();
        Chat.Updated += ApplyChanges;
    }

    private void ApplyChanges()
    {
        Dispatcher.UIThread.Post(() =>
        {
            ChatNameTextBlock.Text = Chat.Name;
        });
        
    }

    private async void LoadMessages()
    {
        await ChatsService.Instance.LoadMessages(Chat.Id, 100);
    }

    private void OnItemInserted(int index, Message obj)
    {
        if (_bubbles.ContainsKey(obj.Id))
            return;
        Dispatcher.UIThread.Post(() =>
        {
            var widget = new Bubble(obj);
            _bubbles[widget.Message.Id] = widget;
            BubblesStackPanel.Children.Insert(index, widget);
        });
    }

    private void OnItemRemoved(int index, Message obj)
    {
        if (!_bubbles.ContainsKey(obj.Id))
            return;
        Dispatcher.UIThread.Post(() =>
        {
            BubblesStackPanel.Children.Remove(_bubbles[obj.Id]);
        });
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ChatsService.Instance.Current = null;
        SettingsButton.IsChecked = false;
    }

    private void SettingsButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        MainView.IsVisible = SettingsButton.IsChecked == false;
        ChatSettingsView.IsVisible = SettingsButton.IsChecked != false;
        if (SettingsButton.IsChecked == false)
        {
            ChatSettingsView.Save();
        }
    }

    private async void SendButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var text = InputBox.Text;
        if (String.IsNullOrEmpty(text))
            return;
        InputBox.Text = "";
        await ChatsService.Instance.CreateMessage(Chat, "user", text, true);
    }
}