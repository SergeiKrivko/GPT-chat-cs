﻿using System.Collections.ObjectModel;
using Auth;
using Core.RemoteRepository;
using Core.RemoteRepository.Models;
using Microsoft.Extensions.Logging;
using Utils;

namespace Core;

public class ChatsService
{
    private static ChatsService? _instance;
    private LocalRepository.LocalRepository _localRepository = LocalRepository.LocalRepository.Instance;
    private ILogger _logger = LogService.CreateLogger("Chats");

    private ChatsService()
    {
        AuthService.Instance.UserChanged += OnUserChanged;
        ChatSocketService.Instance.ChatAdded += OnChatAdded;
        ChatSocketService.Instance.ChatUpdated += OnChatUpdated;
        ChatSocketService.Instance.MessageAdded += OnMessageAdded;
        ChatSocketService.Instance.MessageContentAdded += OnMessageContentAdded;
        OnUserChanged(AuthService.Instance.User);
    }

    private async void OnUserChanged(User? user)
    {
        _logger.LogDebug("Clearing chats");
        Chats.Clear();
        if (user != null)
        {
            _logger.LogDebug("Init local repository");
            await _localRepository.Init();
            _logger.LogDebug("Loading chats from local repository");
            await _loadLocalChats();
            if (AuthService.Instance.User != null && AuthService.Instance.Refreshed)
                OnUserReady(AuthService.Instance.User);
            else
            {
                AuthService.Instance.UserReady += OnUserReady;
            }
        }
    }

    private async void OnUserReady(User user)
    {
        AuthService.Instance.UserReady -= OnUserReady;
        // var timeStamp = SettingsService.Instance.Get<DateTime>($"{user.Id}-timestamp");
        await ChatSocketService.Instance.Connect();
        // await ChatSocketService.Instance.RequestUpdates();
        // await ChatHttpService.Instance.Connect(timeStamp);
        // await MessageHttpService.Instance.Connect(timeStamp);
    }

    public static ChatsService Instance
    {
        get
        {
            _instance ??= new ChatsService();
            return _instance;
        }
    }
    
    private Chat? _current;
    
    public delegate void ChatChangeHandler(Chat? chat);
    public event ChatChangeHandler? CurrentChanged;
    
    public Chat? Current
    {
        get => _current;
        set
        {
            if (value == null || Chats.Contains(value))
            {
                _current = value;
                if (_current == null)
                    _logger.LogDebug($"Current chat changed to null");
                else
                    _logger.LogDebug($"Current chat changed to '{_current?.Id}'");
                CurrentChanged?.Invoke(Current);
            }
            else
            {
                _logger.LogWarning($"Chat '{value.Id}' not found");
            }
        }
    }

    public ObservableCollection<Chat> Chats { get; } = new();

    public Chat GetChat(Guid chatId)
    {
        foreach (var chat in Chats)
        {
            if (chat.Id == chatId)
                return chat;
        }

        throw new KeyNotFoundException($"Chat '{chatId}' not found");
    }

    public async Task CreateChat()
    {
        _logger.LogDebug("Creating new chat...");
        await ChatSocketService.Instance.CreateChat();
    }

    private async void OnChatAdded(Chat chat)
    {
        _logger.LogDebug($"Adding chat '{chat.Id}'...");
        await _localRepository.InsertChat(chat);
        Chats.Add(chat);
    }

    private async void OnChatUpdated(Chat chat)
    {
        _logger.LogDebug($"Updating chat '{chat.Id}'...");
        await _localRepository.SaveChat(chat);
        GetChat(chat.Id).Update(chat);
    }

    public async void SaveChat(Chat chat)
    {
        _logger.LogDebug($"Saving chat '{chat.Id}'...");
        await ChatSocketService.Instance.UpdateChat(chat.Id, new ChatUpdateModel
        {
            name = chat.Name,
            model = chat.Model,
            context_size = chat.ContextSize,
            temperature = chat.Temperature,
        });
    }

    public void SaveChat(Guid chatId)
    {
        SaveChat(GetChat(chatId));
    }

    private async Task _loadLocalChats()
    {
        foreach (var chat in await _localRepository.GetAllChats())
        {
            Chats.Add(chat);
        }
    }
    
    public async Task CreateMessage(Chat chat, string role, string content, bool prompt = false)
    {
        _logger.LogDebug("Creating new message...");
        await ChatSocketService.Instance.CreateMessage(new MessageCreateModel
        {
            chat_uuid =chat.Id,
            role = role,
            content = content,
            model = chat.Model,
            temperature = chat.Temperature,
        }, prompt);
    }

    private async void OnMessageAdded(Message message)
    {
        _logger.LogDebug($"Adding message '{message.Id}'...");
        await _localRepository.InsertMessage(message);
        var chat = GetChat(message.ChatId);
        chat.Messages.Add(message);
    }

    private async void OnMessageContentAdded(Guid chatId, Guid messageId, string content)
    {
        _logger.LogDebug($"Adding content to '{messageId}'...");
        var chat = GetChat(chatId);
        var message = chat.GetMessage(messageId);
        message.AddContent(content);
        await _localRepository.SaveMessage(message);
    }

    public async Task LoadMessages(Guid chatId, int count)
    {
        _logger.LogDebug($"Load messages to chat '{chatId}' from local repository...");
        var chat = GetChat(chatId);
        var messages = await _localRepository.GetAllMessages(chatId);
        if (messages.Count > count)
            messages = messages.Slice(messages.Count - count, count);
        foreach (var message in messages)
        {
            chat.Messages.Insert(0, message);
        }
    }
}