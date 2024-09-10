using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Core;

namespace GptChat.Views;

public partial class ChatsListItem : UserControl
{
    public Chat Chat { get; }

    public delegate void SelectHandler(Chat chat);

    public event SelectHandler? Selected;

    public bool IsSelected
    {
        get => MainButton.IsChecked ?? false;
        set => MainButton.IsChecked = value;
    }
    
    public ChatsListItem(Chat chat)
    {
        Chat = chat;
        InitializeComponent();
        Update();
    }

    private void Update()
    {
        ChatNameBlock.Text = Chat.Name;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Selected?.Invoke(Chat);
    }

    private async void DeleteChat_OnClick(object? sender, RoutedEventArgs e)
    {
        await ChatsService.Instance.DeleteChat(Chat.Id);
    }
}