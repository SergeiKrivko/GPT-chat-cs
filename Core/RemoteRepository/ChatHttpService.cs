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

    public delegate void NewChatHeader(Chat chat);
    public event NewChatHeader? NewChat;

    public delegate void DeleteChatHeader(Guid chatId);
    public event DeleteChatHeader? DeleteChat;

    private ChatHttpService()
    {
        BaseUrl = "http://127.0.0.1:8000";
        Token = AuthService.Instance.User?.IdToken;
        AuthService.Instance.UserChanged += OnUserChanged;
        OnUserChanged(AuthService.Instance.User);

        _socketService = new ChatSocketService();
        
    }

    public async Task<List<GetChatResponseBody>> GetAllChats()
    {
        try
        {
            return await Get<List<GetChatResponseBody>>("/api/v1/chats");
        }
        catch (NotFoundException)
        {
            return new();
        }
    }

    public async Task<List<GetChatResponseBody>> GetAllChatsCreatedAfter(DateTime time)
    {
        try
        {
            return await Get<List<GetChatResponseBody>>($"/api/v1/chats?created_after={time:s}");
        }
        catch (NotFoundException)
        {
            return new();
        }
    }

    public async Task<List<GetChatResponseBody>> GetAllChatsDeletedAfter(DateTime time)
    {
        try
        {
            return await Get<List<GetChatResponseBody>>($"/api/v1/chats?deleted_after={time:s}");
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
            NewChat?.Invoke(new Chat
            {
                Id = chat.uuid,
                CreatedAt = chat.created_at,
                DeletedAt = chat.deleted_at,
                Name = chat.name,
                Model = chat.model,
                ContextSize = chat.context_size ?? 0,
                Temperature = chat.temperature ?? 0.5,
            });
        }

        foreach (var chat in await GetAllChatsDeletedAfter(timeStamp))
        {
            DeleteChat?.Invoke(chat.uuid);
        }
    }

    public async Task Connect()
    {
        var user = AuthService.Instance.User;
        while (true)
        {
            if (user == null)
                break;
            if (!AuthService.Instance.Refreshed)
            {
                await Task.Delay(100);
                continue;
            }
            var timeStamp = SettingsService.Instance.Get<DateTime>($"{user.Id}-timestamp");
            Token = user?.IdToken;
            try
            {
                await LoadChats(timeStamp);
                SettingsService.Instance.Set($"{user?.Id}-timestamp", DateTime.Now);
                await _socketService.Connect();
                _socketService.Subscribe<List<GetChatResponseBody>>("new_chats", chats =>
                {
                    Console.WriteLine($"Socket: {chats.Count}");
                    foreach (var chat in chats)
                    {
                        NewChat?.Invoke(new Chat
                        {
                            Id = chat.uuid,
                            CreatedAt = chat.created_at,
                            DeletedAt = chat.deleted_at,
                            Name = chat.name,
                            Model = chat.model,
                            ContextSize = chat.context_size ?? 0,
                            Temperature = chat.temperature ?? 0.5,
                        });
                    }
                });
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
}