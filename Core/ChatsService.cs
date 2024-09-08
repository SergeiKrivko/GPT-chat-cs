using System.Collections.ObjectModel;
using Auth;
using Core.RemoteRepository;
using Core.RemoteRepository.Models;
using Utils;

namespace Core;

public class ChatsService
{
    private static ChatsService? _instance;
    private LocalRepository.LocalRepository _localRepository = LocalRepository.LocalRepository.Instance;

    private ChatsService()
    {
        ChatHttpService.Instance.ChatAdded += OnChatAdded;
        ChatHttpService.Instance.ChatUpdated += OnChatUpdated;
        AuthService.Instance.UserChanged += OnUserChanged;
        MessageHttpService.Instance.MessageAdded += OnMessageAdded;
        MessageHttpService.Instance.MessageContentAdded += OnMessageContentAdded;
        OnUserChanged(AuthService.Instance.User);
    }

    private async void OnUserChanged(User? user)
    {
        await _localRepository.Init();
        await _loadLocalChats();
        while (!AuthService.Instance.Refreshed)
        {
            await Task.Delay(100);
        }
        var timeStamp = SettingsService.Instance.Get<DateTime>($"{user.Id}-timestamp");
        await ChatSocketService.Instance.Connect();
        await ChatHttpService.Instance.Connect(timeStamp);
        await MessageHttpService.Instance.Connect(timeStamp);
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
            }
            else
            {
                throw new Exception("Unknown project!");
            }
            CurrentChanged?.Invoke(Current);
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

        throw new KeyNotFoundException($"Chat {chatId} not found");
    }

    public async Task CreateChat()
    {
        await ChatHttpService.Instance.CreateChat();
    }

    private async void OnChatAdded(Chat chat)
    {
        await _localRepository.InsertChat(chat);
        Chats.Add(chat);
    }

    private async void OnChatUpdated(Chat chat)
    {
        await _localRepository.SaveChat(chat);
        GetChat(chat.Id).Update(chat);
    }

    public async void SaveChat(Chat chat)
    {
        await ChatHttpService.Instance.UpdateChat(chat.Id, new ChatUpdateModel
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
        await MessageHttpService.Instance.CreateMessage(new MessageCreateModel
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
        await _localRepository.InsertMessage(message);
        var chat = GetChat(message.ChatId);
        chat.Messages.Add(message);
    }

    private async void OnMessageContentAdded(Guid chatId, Guid messageId, string content)
    {
        var chat = GetChat(chatId);
        var message = chat.GetMessage(messageId);
        message.AddContent(content);
        await _localRepository.SaveMessage(message);
    }

    public async Task LoadMessages(Guid chatId, int count)
    {
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