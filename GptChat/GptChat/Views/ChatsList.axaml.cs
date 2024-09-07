using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Core;

namespace GptChat.Views;

public partial class ChatsList : UserControl
{
    public ChatsService Service { get; } = ChatsService.Instance;
    
    public ChatsList()
    {
        InitializeComponent();
        ChatsListBox.ItemsSource = Service.Chats;
        ChatsService.Instance.CurrentChanged += OnCurrentChatChanged;
    }

    private void OnCurrentChatChanged(Chat? chat)
    {
        if (ChatsListBox.SelectedItem != chat)
        {
            ChatsListBox.SelectedItem = chat;
        }
    }

    private async void CreateChatButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ChatsService.Instance.CreateChat();
    }

    private void ChatsListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = ChatsListBox.SelectedItem;
        if (item == null)
        {
            ChatsService.Instance.Current = null;
        }
        else
        {
            ChatsService.Instance.Current = (Chat)item;
        }
    }
}