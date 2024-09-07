using System;
using System.IO;
using Avalonia.Controls;
using Core;

namespace GptChat.Views;

public partial class ChatSettings : UserControl
{
    public Chat? Chat { get; set; }
    
    public ChatSettings()
    {
        InitializeComponent();
    }

    public void Update()
    {
        if (Chat == null)
            return;
        ChatNameBox.Text = Chat.Name;
        ChatModelBox.SelectedItem = Chat.Model;
        ChatSyncSwitch.IsChecked = Chat.Sync;
    }

    public void Save()
    {
        if (Chat == null)
            return;
        var flag = false;
        flag |= Chat.Name != ChatNameBox.Text;
        // flag |= Chat.Model != (string?)ChatModelBox.SelectedValue;
        flag |= Chat.Sync != ChatSyncSwitch.IsChecked;
        if (flag)
        {
            Chat.Name = ChatNameBox.Text ?? "";
            // Chat.Model = (string?)ChatModelBox.SelectedValue;
            Chat.Sync = ChatSyncSwitch.IsChecked ?? false;
            ChatsService.Instance.SaveChat(Chat);
        }
    }
}