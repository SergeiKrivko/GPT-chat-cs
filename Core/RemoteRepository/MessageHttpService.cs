using System.Net.Http.Headers;
using Auth;
using Core.RemoteRepository.Models;
using Utils;
using Utils.Http;
using Utils.Http.Exceptions;

namespace Core.RemoteRepository;

public class MessageHttpService: BodyDetailHttpService
{
    private static MessageHttpService? _instance;

    public static MessageHttpService Instance
    {
        get
        {
            _instance ??= new MessageHttpService();
            return _instance;
        }
    }
    
    private ChatSocketService _socketService;

    public delegate void MessageHeader(Message message);
    public event MessageHeader? MessageAdded;
    public event MessageHeader? MessageUpdated;

    public delegate void DeleteMessageHeader(Guid messageId);
    public event DeleteMessageHeader? MessageDeleted;

    private MessageHttpService()
    {
        BaseUrl = "http://127.0.0.1:8000";
        Token = AuthService.Instance.User?.IdToken;
        AuthService.Instance.UserChanged += OnUserChanged;
        OnUserChanged(AuthService.Instance.User);

        _socketService = ChatSocketService.Instance;
        _socketService.Subscribe<List<MessageReadModel>>("new_messages", messages =>
        {
            foreach (var message in messages)
            {
                MessageAdded?.Invoke(Message.FromReadModel(message));
            }
        });
        _socketService.Subscribe<MessageReadModel>("update_message", message =>
        {
            Console.WriteLine("Updating message...");
            MessageUpdated?.Invoke(Message.FromReadModel(message));
        });
        
    }

    public async Task<List<MessageReadModel>> GetAllMessages()
    {
        try
        {
            return await Get<List<MessageReadModel>>("/api/v1/messages");
        }
        catch (NotFoundException)
        {
            return new();
        }
    }

    public async Task<List<MessageReadModel>> GetAllMessagesCreatedAfter(DateTime time)
    {
        try
        {
            return await Get<List<MessageReadModel>>($"/api/v1/messages?created_after={time:s}");
        }
        catch (NotFoundException)
        {
            return new();
        }
    }

    public async Task<List<MessageReadModel>> GetAllMessagesDeletedAfter(DateTime time)
    {
        try
        {
            return await Get<List<MessageReadModel>>($"/api/v1/messages?deleted_after={time:s}");
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

    private async Task LoadMessages(DateTime timeStamp)
    {
        foreach (var message in await GetAllMessagesCreatedAfter(timeStamp))
        {
            MessageAdded?.Invoke(Message.FromReadModel(message));
        }

        foreach (var message in await GetAllMessagesDeletedAfter(timeStamp))
        {
            MessageDeleted?.Invoke(message.uuid);
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
                await LoadMessages(timeStamp);
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

    public async Task CreateMessage(MessageCreateModel model)
    {
        await _socketService.Emit("new_message", model);
    }
}