﻿using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Core;
using Core.RemoteRepository;
using SocketIOClient;

namespace GptChat.Windows;

public partial class ChatSettings : Window
{
    public Chat? Chat { get; set; }
    private List<string> Models { get; } = new();
    
    public ChatSettings()
    {
        InitializeComponent();
        GetModels();
    }

    private async void GetModels()
    {
        while (true)
        {
            try
            {
                Models.AddRange(await GptHttpService.Instance.GetModels());
                ChatModelBox.ItemsSource = Models;
                break;
            }
            catch (ConnectionException)
            {
            }
        }
    }

    public void Update()
    {
        if (Chat == null)
            return;
        ChatNameBox.Text = Chat.Name;
        ChatModelBox.SelectedItem = Chat.Model;
        ContextBox.Value = Chat.ContextSize;
        TemperatureBox.Value = (decimal)Chat.Temperature;
    }

    public void Save()
    {
        if (Chat == null)
            return;
        var flag = false;
        Console.WriteLine(ChatModelBox.SelectedValue);
        flag |= Chat.Name != ChatNameBox.Text;
        flag |= Chat.Model != (string?)ChatModelBox.SelectedValue;
        flag |= Chat.ContextSize != ContextBox.Value;
        flag |= Chat.Temperature != TemperatureBox.Value;
        if (flag)
        {
            Chat.Name = ChatNameBox.Text ?? "";
            Chat.Model = (string?)ChatModelBox.SelectedValue;
            Chat.ContextSize = (int?)ContextBox.Value ?? 0;
            Chat.Temperature = TemperatureBox.Value ?? (decimal)0.5;
            ChatsService.Instance.SaveChat(Chat);
        }
    }

    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        Save();
        Close();
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        // Update();
        Close();
    }
}