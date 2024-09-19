using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Core;
using Core.LocalRepository;
using Core.RemoteRepository;
using Utils.Http.Exceptions;

namespace GptChat.Views;

public partial class Bubble : UserControl
{
    public Message Message { get; }
    private string? _originalLang = null;
    private string? _lang = null;

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

    private async void Update()
    {
        InnerBorder.HorizontalAlignment = Message.Role == "user" ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        InnerBorder.CornerRadius = new CornerRadius(10, 10,
            Message.Role == "user" ? 0 : 10,
            Message.Role == "user" ? 10 : 0);
        GptBackground.CornerRadius = InnerBorder.CornerRadius;
        UserBackground.CornerRadius = InnerBorder.CornerRadius;
        UserBackground.IsVisible = Message.Role == "user";
        GptBackground.IsVisible = Message.Role != "user";
        MarkdownViewer.Markdown = Message.Content;

        foreach (var reply in Message.Reply.FindAll(r => r.Type == "explicit"))
        {
            ReplyPanel.Children.Add(new ReplyItem(await LocalRepository.Instance.GetMessage(reply.ReplyTo)));
            ReplyPanel.IsVisible = true;
        }
    }

    private async void CopyText_OnClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(MarkdownViewer.Markdown);
        }
    }

    private async void DeleteMessage_OnClick(object? sender, RoutedEventArgs e)
    {
        await ChatsService.Instance.DeleteMessage(Message.Id);
    }

    private async void Translate(string dst)
    {
        try
        {
            var res = await TranslateHttpService.Instance.Translate(Message.Content, dst);
            MarkdownViewer.Markdown = res.res;
            TranslatedWidget.IsVisible = true;
            TranslatedFromBlock.Text = $"Переведено с {res.src}";
            _lang = res.dst;
        }
        catch (HttpServiceException e)
        {
            Console.WriteLine(e);
        }
    }
    private void TranslateToRussianItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Translate("rus");
    }

    private void TranslateToFrench_OnClick(object? sender, RoutedEventArgs e)
    {
        Translate("fra");
    }

    private void TranslateToGerman_OnClick(object? sender, RoutedEventArgs e)
    {
        Translate("eng");
    }

    private void TranslateToSpanish_OnClick(object? sender, RoutedEventArgs e)
    {
        Translate("esp");
    }

    private void TranslateToItalian_OnClick(object? sender, RoutedEventArgs e)
    {
        Translate("ita");
    }

    private void TranslateToEnglish_OnClick(object? sender, RoutedEventArgs e)
    {
        Translate("eng");
    }

    private async void MenuBase_OnOpened(object? sender, RoutedEventArgs e)
    {
        TranslateToRussianItem.IsVisible = false;
        if (_originalLang == null)
        {
            try
            {
                _lang = await TranslateHttpService.Instance.DetectLang(Message.Content);
                _originalLang = _lang;
            }
            catch (HttpServiceException exception)
            {
                Console.WriteLine(exception);
            }
        }
        TranslateToRussianItem.IsVisible = _lang != "rus";
    }

    private void ShowOriginal_OnClick(object? sender, RoutedEventArgs e)
    {
        MarkdownViewer.Markdown = Message.Content;
        _lang = _originalLang;
        TranslatedWidget.IsVisible = false;
    }

    public delegate void ReplyClickHandler(Message message);

    public event ReplyClickHandler? ReplyClicked;

    private void Reply_OnClick(object? sender, RoutedEventArgs e)
    {
        ReplyClicked?.Invoke(Message);
    }
}