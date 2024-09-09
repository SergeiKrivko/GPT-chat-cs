using System.Net.Http.Headers;
using Auth;
using Core.RemoteRepository.Models;
using Utils;
using Utils.Http;
using Utils.Http.Exceptions;

namespace Core.RemoteRepository;

public class ChatHttpService: BodyDetailHttpService
{
    private static ChatHttpService? _instance;

    public static ChatHttpService Instance
    {
        get
        {
            _instance ??= new ChatHttpService();
            return _instance;
        }
    }
    
    private ChatSocketService _socketService;

    public delegate void ChatHeader(Chat chat);
    public event ChatHeader? ChatAdded;
    public event ChatHeader? ChatUpdated;

    public delegate void DeleteChatHeader(Guid chatId);
    public event DeleteChatHeader? ChatDeleted;

    private ChatHttpService()
    {
        BaseUrl = Config.BaseUrl;
        Token = AuthService.Instance.User?.IdToken;
        AuthService.Instance.UserChanged += OnUserChanged;
        OnUserChanged(AuthService.Instance.User);

        _socketService = ChatSocketService.Instance;
        _socketService.Subscribe<List<ChatReadModel>>("new_chats", chats =>
        {
            foreach (var chat in chats)
            {
                ChatAdded?.Invoke(Chat.FromReadModel(chat));
            }
        });
        _socketService.Subscribe<ChatReadModel>("update_chat", chat =>
        {
            Console.WriteLine("Updating chat...");
            ChatUpdated?.Invoke(Chat.FromReadModel(chat));
        });
        
    }

    public async Task<List<ChatReadModel>> GetAllChats()
    {
        try
        {
            return await Get<List<ChatReadModel>>("/api/v1/chats");
        }
        catch (NotFoundException)
        {
            return new();
        }
    }

    public async Task<List<ChatReadModel>> GetAllChatsCreatedAfter(DateTime time)
    {
        try
        {
            return await Get<List<ChatReadModel>>($"/api/v1/chats?created_after={time:s}");
        }
        catch (NotFoundException)
        {
            return new();
        }
    }

    public async Task<List<ChatReadModel>> GetAllChatsDeletedAfter(DateTime time)
    {
        try
        {
            return await Get<List<ChatReadModel>>($"/api/v1/chats?deleted_after={time:s}");
        }
        catch (NotFoundException)
        {
            return new();
        }
    }

    private async void OnUserChanged(User? user)
    {
        Token = user?.IdToken;
    }

    private async Task LoadChats(DateTime timeStamp)
    {
        foreach (var chat in await GetAllChatsCreatedAfter(timeStamp))
        {
            ChatAdded?.Invoke(Chat.FromReadModel(chat));
        }

        foreach (var chat in await GetAllChatsDeletedAfter(timeStamp))
        {
            ChatDeleted?.Invoke(chat.uuid);
        }
    }

    public async Task Connect(DateTime timeStamp)
    {
        var user = AuthService.Instance.User;
        while (true)
        {
            if (user == null)
                break;
            Token = user.IdToken;
            try
            {
                await LoadChats(timeStamp);
                SettingsService.Instance.Set($"{user?.Id}-timestamp", DateTime.Now);
                break;
            }
            catch (ConnectionException)
            {
                Console.WriteLine("Updates loading failed");
            }

            await Task.Delay(20000);
            Console.WriteLine("Trying to load updates...");
        }
    }

    public async Task CreateChat()
    {
        await _socketService.Emit("new_chat");
    }

    public async Task UpdateChat(Guid id, ChatUpdateModel chat)
    {
        await _socketService.Emit("update_chat", id, chat);
    }
}