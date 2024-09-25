using System.Collections.ObjectModel;
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
    private bool _sorting = false;

    private ChatsService()
    {
        AuthService.Instance.UserChanged += OnUserChanged;
        ChatSocketService.Instance.ChatAdded += OnChatAdded;
        ChatSocketService.Instance.ChatUpdated += OnChatUpdated;
        ChatSocketService.Instance.ChatDeleted += OnChatDeleted;
        ChatSocketService.Instance.MessageAdded += OnMessageAdded;
        ChatSocketService.Instance.MessageDeleted += OnMessageDeleted;
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
            await LoadLocalChats();
            SortChats();
            if (AuthService.Instance.User != null && AuthService.Instance.Refreshed)
                OnUserReady(AuthService.Instance.User);
            else
            {
                AuthService.Instance.UserReady += OnUserReady;
            }

            await Task.Delay(100);
            SortChats();
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

    public ObservableList<Chat> Chats { get; } = new();

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

    public async Task DeleteChat(Guid chatId)
    {
        _logger.LogDebug($"Deleting chat '{chatId}'...");
        await ChatSocketService.Instance.DeleteChat(chatId);
    }

    private async void OnChatAdded(Chat chat)
    {
        _logger.LogDebug($"Adding chat '{chat.Id}'...");
        await _localRepository.InsertChat(chat);
        Chats.Add(chat);
        
        if (!ChatSocketService.Instance.LoadingUpdates)
        {
            await Task.Delay(100);
            SortChats();
        }
    }

    private async void OnChatUpdated(Chat chat)
    {
        _logger.LogDebug($"Updating chat '{chat.Id}'...");
        await _localRepository.SaveChat(chat);
        GetChat(chat.Id).Update(chat);
        
        if (!ChatSocketService.Instance.LoadingUpdates)
        {
            SortChats();
        }
    }

    private async void OnChatDeleted(Guid chatId)
    {
        _logger.LogDebug($"Deleting chat '{chatId}'...");
        try
        {
            Chats.Remove(GetChat(chatId));
            await _localRepository.RemoveChat(chatId);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning($"Chat '{chatId} not found");
        }
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
            pinned = chat.Pinned,
            archived = chat.Archived,
        });
    }

    public void SaveChat(Guid chatId)
    {
        SaveChat(GetChat(chatId));
    }

    private async Task LoadLocalChats()
    {
        foreach (var chat in await _localRepository.GetAllChats())
        {
            Chats.Add(chat);
        }
    }
    
    public async Task CreateMessage(Chat chat, string role, string content, List<Message>? reply = null, bool prompt = false)
    {
        _logger.LogDebug($"Creating new message with {reply?.Count} reply ...");

        var replys = new List<ReplyCreateModel>();
        foreach (var message in reply ?? new())
        {
            replys.Add(new ReplyCreateModel()
            {
                reply_to = message.Id,
                type = "explicit",
            });
        }
        
        await ChatSocketService.Instance.CreateMessage(new MessageCreateModel
        {
            chat_uuid =chat.Id,
            role = role,
            content = content,
            model = chat.Model,
            temperature = chat.Temperature,
            reply = replys,
        }, prompt);
    }

    public async Task DeleteMessage(Guid messageId)
    {
        _logger.LogDebug($"Deleting message '{messageId}'...");
        await ChatSocketService.Instance.DeleteMessage(messageId);
    }

    private async void OnMessageAdded(Message message)
    {
        _logger.LogDebug($"Adding message '{message.Id}'...");
        await _localRepository.InsertMessage(message);
        var chat = GetChat(message.ChatId);
        chat.Messages.Add(message);
        
        if (!ChatSocketService.Instance.LoadingUpdates)
        {
            await Task.Delay(100);
            SortChats();
        }
    }

    private async void OnMessageContentAdded(Guid chatId, Guid messageId, string content)
    {
        _logger.LogDebug($"Adding content to '{messageId}'...");
        var chat = GetChat(chatId);
        var message = chat.GetMessage(messageId);
        message.AddContent(content);
        await _localRepository.SaveMessage(message);
    }

    private async void OnMessageDeleted(Guid messageId)
    {
        Message? toRemove = null;
        foreach (var chat in Chats)
        {
            foreach (var message in chat.Messages)
            {
                if (message.Id == messageId)
                {
                    toRemove = message;
                    break;
                }
            }
            if (toRemove != null)
            {
                chat.Messages.Remove(toRemove);
                break;
            }
        }
        await _localRepository.RemoveMessage(messageId);
    }

    public async Task LoadMessages(Guid chatId, int count)
    {
        _logger.LogDebug($"Load messages to chat '{chatId}' from local repository...");
        var chat = GetChat(chatId);
        
        List<Message> messages;
        if (chat.LastLoadedMessage == null)
            messages = await _localRepository.GetAllMessages(chatId);
        else
            messages = await _localRepository.GetAllMessagesBefore(chatId, chat.LastLoadedMessage.Value);
        _logger.LogDebug($"Found {messages.Count} messages");
        
        if (messages.Count > count)
            messages = messages.Slice(messages.Count - count, count);
        messages.Reverse();
        _logger.LogDebug($"Adding {messages.Count} messages");
        foreach (var message in messages)
        {
            chat.Messages.Insert(0, message);
            chat.LastLoadedMessage = message.Id;
        }
    }

    public void UnloadMessages(Chat chat)
    {
        var len = chat.Messages.Count - 5;
        for (var i = 0; i < len; i++)
        {
            chat.Messages.Pop(0);
        }

        chat.LastLoadedMessage = chat.Messages.Count > 0 ? chat.Messages[0].Id : null;
        _logger.LogDebug($"Unload messages from '{chat.Name}' until = {chat.LastLoadedMessage}");
    }

    private void SortChats()
    {
        if (_sorting)
            return;
        _sorting = true;
        _logger.LogDebug("Start sorting chats...");
        var chats = Chats.ToList();
        chats.Sort();
        foreach (var chat in chats)
        {
            Chats.MoveItem(chat, 0);
        }
        _logger.LogDebug("Chats sorted...");
        _sorting = false;
    }
}