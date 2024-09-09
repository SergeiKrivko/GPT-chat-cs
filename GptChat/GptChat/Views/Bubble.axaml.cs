using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using Core;

namespace GptChat.Views;

public partial class Bubble : UserControl
{
    public Message Message { get; }
    
    public Bubble(Message message)
    {
        Message = message;
        Message.Updated += OnMessageOnUpdated;
        InitializeComponent();
        Update();
    }

    private void OnMessageOnUpdated()
    {
        Dispatcher.UIThread.Post(() => MarkdownViewer.Markdown = Message.Content);
    }

    public void Update()
    {
        InnerBorder.HorizontalAlignment = Message.Role == "user" ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        InnerBorder.CornerRadius = new CornerRadius(10, 10, 
            Message.Role == "user" ? 0 : 10, 
            Message.Role == "user" ? 10 : 0);
        // TextBlock.Text = Message?.Content;
        MarkdownViewer.Markdown = Message.Content;
    }

    private async void CopyText_OnClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(MarkdownViewer.Markdown);
        }
    }
}