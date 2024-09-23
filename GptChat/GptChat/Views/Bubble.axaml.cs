using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Core;
using Core.LocalRepository;
using Core.RemoteRepository;
using Core.Search;
using Utils.Http.Exceptions;

namespace GptChat.Views;

public partial class Bubble : UserControl
{
    public Message Message { get; }
    private string? _originalLang = null;
    private string? _lang;
    public List<SearchResult> SearchResults { get; } = new();

    public double MaxBubbleWidth
    {
        set => InnerBorder.MaxWidth = value;
    }

    public Bubble(Message message)
    {
        Message = message;
        Message.Updated += OnMessageOnUpdated;
        InitializeComponent();
        if (Message.Transaction != null)
        {
            _lang = Message.Transaction.DstLang;
            _originalLang = Message.Transaction.SrcLang;
        }

        Update();
    }

    private void OnMessageOnUpdated()
    {
        Dispatcher.UIThread.Post(() => MarkdownViewer.Markdown = Message.Content);
    }

    public async void Update()
    {
        InnerBorder.HorizontalAlignment = Message.Role == "user" ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        InnerBorder.CornerRadius = new CornerRadius(10, 10,
            Message.Role == "user" ? 0 : 10,
            Message.Role == "user" ? 10 : 0);
        GptBackground.CornerRadius = InnerBorder.CornerRadius;
        UserBackground.CornerRadius = InnerBorder.CornerRadius;
        UserBackground.IsVisible = Message.Role == "user";
        GptBackground.IsVisible = Message.Role != "user";

        TranslatedWidget.IsVisible = Message.Transaction != null;
        TranslatedFromBlock.Text = $"Переведено с {Message.Transaction?.SrcLang}";

        foreach (var reply in Message.Reply.FindAll(r => r.Type == "explicit"))
        {
            var item = new ReplyItem(await LocalRepository.Instance.GetMessage(reply.ReplyTo));
            item.Removable = false;
            item.ScrollClicked += id => ScrollRequested?.Invoke(id);
            ReplyPanel.Children.Add(item);
            ReplyPanel.IsVisible = true;
        }

        var markdown = Message.Text;
        foreach (var result in SearchResults.OrderBy(r => r.Offset).Reverse())
        {
            if (Application.Current != null)
            {
                Application.Current.Resources.TryGetResource(
                    result.Selected ? "SelectedSearchResultColor" : "SearchResultColor", null, out var color);
                markdown = markdown.Insert(result.Offset + result.Len, "%")
                    .Insert(result.Offset, $"%{{background:{color}}}");
            }
        }

        MarkdownViewer.Markdown = markdown;
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
            await LocalRepository.Instance.AddTranslation(Message, res.res, res.src, res.dst);
            _lang = res.dst;
            Update();
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

        TranslateToRussianItem.IsVisible = _lang != "rus" && _originalLang != "rus";
    }

    private async void ShowOriginal_OnClick(object? sender, RoutedEventArgs e)
    {
        await LocalRepository.Instance.RemoveTranslation(Message);
        _lang = _originalLang;
        Update();
    }

    public delegate void ReplyClickHandler(Message message);

    public event ReplyClickHandler? ReplyClicked;

    private void Reply_OnClick(object? sender, RoutedEventArgs e)
    {
        ReplyClicked?.Invoke(Message);
    }

    public delegate void ScrollClickHandler(Guid id);

    public event ScrollClickHandler? ScrollRequested;
}