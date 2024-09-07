using System.Collections.ObjectModel;

namespace Core;

public class ChatsService
{
    private static ChatsService? _instance;
    private LocalRepository _localRepository = LocalRepository.Instance;

    private ChatsService()
    {
        _loadChats();
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

    public async Task<Chat> CreateChat()
    {
        var chat = new Chat();
        chat.Id = Guid.NewGuid();
        await _localRepository.InsertChat(chat);
        Chats.Add(chat);
        return chat;
    }

    public async void SaveChat(Chat chat)
    {
        await _localRepository.SaveChat(chat);
    }

    public async void SaveChat(Guid chatId)
    {
        await _localRepository.SaveChat(GetChat(chatId));
    }

    private async void _loadChats()
    {
        foreach (var chat in await _localRepository.GetAllChats())
        {
            Chats.Add(chat);
        }
    }

    public async Task LoadMessages(Guid chatId, int count)
    {
        var chat = GetChat(chatId);
        var messages = await _localRepository.GetAllMessages(chatId, maxIndex: chat.LastLoadedMessage - 1);
        if (messages.Count > count)
            messages = messages.Slice(messages.Count - count, count);
        foreach (var message in messages)
        {
            chat.Messages.Insert(0, message);
        }
    }
}