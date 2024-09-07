using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Core;

namespace GptChat.Views;

public partial class Bubble : UserControl
{
    public Message Message { get; }
    
    public Bubble(Message message)
    {
        Message = message;
        InitializeComponent();
        Update();
    }

    public void Update()
    {
        InnerBorder.HorizontalAlignment = Message.Role == "user" ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        InnerBorder.CornerRadius = new CornerRadius(10, 10, 
            Message.Role == "user" ? 10 : -0, 
            Message.Role == "user" ? 0 : 10);
        // TextBlock.Text = Message?.Content;
        MarkdownViewer.Markdown = Message.Content;
    }
}