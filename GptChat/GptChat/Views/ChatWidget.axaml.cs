﻿using System;
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
        Chat.Messages.CollectionChanged += ChatsOnCollectionChanged;
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

    private void ChatsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var widget = new Bubble((Message)item);
                    _bubbles[widget.Message.Id] = widget;
                    BubblesStackPanel.Children.Add(widget);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    BubblesStackPanel.Children.Remove(_bubbles[((Message)item).Id]);
                }
            }
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