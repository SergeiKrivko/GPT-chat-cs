using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Core;

namespace GptChat.Views;

public partial class ReplyItem : UserControl
{
    private bool _removable = false;
    public bool Removable
    {
        get => _removable;
        set
        {
            _removable = value;
            RemoveButton.IsVisible = value;
        }
    }
    
    public Message Message { get; }

    public ReplyItem(Message message)
    {
        Message = message;
        InitializeComponent();

        MessageTextBlock.Text = message.Content;
    }

    public delegate void RemoveClickHandler(Message message);

    public event RemoveClickHandler? RemoveClicked;

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        RemoveClicked?.Invoke(Message);
    }
}