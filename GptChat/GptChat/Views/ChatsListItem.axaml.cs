using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Core;

namespace GptChat.Views;

public partial class ChatsListItem : UserControl
{
    public Chat Chat { get; }

    private IObservable<object?> _resource;
    private IDisposable? _subscription;

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
        Chat.Updated += Update;
        Chat.Messages.ItemInserted += (index, message) => UpdateLastMessageText();
        Chat.Messages.ItemRemoved += (index, message) => UpdateLastMessageText();
        
        _resource = Resources.GetResourceObservable("ChatColor0");
    }

    private void Update()
    {
        Dispatcher.UIThread.Post(() =>
        {
            ChatNameBlock.Text = Chat.Name;
            _subscription?.Dispose();
            _resource = Resources.GetResourceObservable($"ChatColor{Chat.Color ?? 0}");
            _subscription = _resource.Subscribe(o =>
            {
                if (!string.IsNullOrEmpty(o?.ToString()) && o.ToString() != "(unset)")
                    CircleBorder.Background = Brush.Parse(o.ToString() ?? "#2B5E2E");
            });
            CircleTextBlock.Text = GenerateText();
            ButtonPin.IsVisible = !Chat.Pinned;
            ButtonUnpin.IsVisible = Chat.Pinned;
            ButtonArchive.IsVisible = !Chat.Archived;
            ButtonUnArchive.IsVisible = Chat.Archived;
            IconPinned.IsVisible = Chat.Pinned;
        });
    }

    private void UpdateLastMessageText()
    {
        Dispatcher.UIThread.Post(() => LastMessageBlock.Text = Chat.LastMessage?.Content.Replace("\n\n", "\n"));
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Selected?.Invoke(Chat);
    }

    private async void DeleteChat_OnClick(object? sender, RoutedEventArgs e)
    {
        await ChatsService.Instance.DeleteChat(Chat.Id);
    }

    private string GenerateText()
    {
        var text = Chat.Name;
        var lst = new List<string>();

        foreach (var word in text.Split())
        {
            if (string.IsNullOrEmpty(word))
                continue;
            lst.Add(word[..1]);
            for (var i = 1; i < word.Length; i++)
            {
                var letter = word[i..(i + 1)];
                if (letter == letter.ToUpper())
                    lst.Add(letter);
            }
        }

        if (lst.Count == 0)
            return "";
        if (lst.Count == 1)
            return lst[0];
        return string.Join(string.Empty, lst.Slice(0, 2));
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // LastMessageBlock.Width = e.NewSize.Width - 60;
    }

    private void ButtonPin_OnClick(object? sender, RoutedEventArgs e)
    {
        Chat.Pinned = true;
        ChatsService.Instance.SaveChat(Chat);
    }

    private void ButtonUnpin_OnClick(object? sender, RoutedEventArgs e)
    {
        Chat.Pinned = false;
        ChatsService.Instance.SaveChat(Chat);
    }

    private void ButtonArchive_OnClick(object? sender, RoutedEventArgs e)
    {
        Chat.Archived = true;
        ChatsService.Instance.SaveChat(Chat);
    }

    private void ButtonUnArchive_OnClick(object? sender, RoutedEventArgs e)
    {
        Chat.Archived = false;
        ChatsService.Instance.SaveChat(Chat);
    }
}