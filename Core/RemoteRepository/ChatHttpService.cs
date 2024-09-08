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

    public async Task LoadChats()
    {
        var user = AuthService.Instance.User;
        if (user == null)
            return;
        var timeStamp1 = SettingsService.Instance.Get<DateTime>($"{user.Id}-timestamp");
        var timeStamp2 = timeStamp1;
        while (true)
        {
            Console.WriteLine(AuthService.Instance.Refreshed);
            if (!AuthService.Instance.Refreshed)
            {
                await Task.Delay(100);
                continue;
            }
            Token = user?.IdToken;
            try
            {
                foreach (var chat in await GetAllChatsCreatedAfter(timeStamp1))
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

                timeStamp1 = DateTime.Now;

                foreach (var chat in await GetAllChatsDeletedAfter(timeStamp2))
                {
                    DeleteChat?.Invoke(chat.uuid);
                }

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
}