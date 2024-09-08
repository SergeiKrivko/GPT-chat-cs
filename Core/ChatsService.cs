using System.Collections.ObjectModel;
using Auth;
using Core.RemoteRepository;

namespace Core;

public class ChatsService
{
    private static ChatsService? _instance;
    private LocalRepository.LocalRepository _localRepository = LocalRepository.LocalRepository.Instance;

    private ChatsService()
    {
        ChatHttpService.Instance.NewChat += NewChat;
        AuthService.Instance.UserChanged += OnUserChanged;
        OnUserChanged(AuthService.Instance.User);
        
    }

    private async void OnUserChanged(User? user)
    {
        await _localRepository.Init();
        await _loadLocalChats();
        await ChatHttpService.Instance.Connect();
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
        var chat = new Chat();
        chat.Id = Guid.NewGuid();
        NewChat(chat);
    }

    public async void NewChat(Chat chat)
    {
        await _localRepository.InsertChat(chat);
        Chats.Add(chat);
    }

    public async void SaveChat(Chat chat)
    {
        await _localRepository.SaveChat(chat);
    }

    public async void SaveChat(Guid chatId)
    {
        await _localRepository.SaveChat(GetChat(chatId));
    }

    private async Task _loadLocalChats()
    {
        foreach (var chat in await _localRepository.GetAllChats())
        {
            Chats.Add(chat);
        }
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