using Auth;
using Core.RemoteRepository.Models;
using Microsoft.Extensions.Logging;
using Utils;
using Utils.Sockets;

namespace Core.RemoteRepository;

public class ChatSocketService : SocketService
{
    private static ChatSocketService? _instance;

    public static ChatSocketService Instance
    {
        get
        {
            _instance ??= new ChatSocketService();
            return _instance;
        }
    }

    public delegate void ChatHeader(Chat chat);

    public event ChatHeader? ChatAdded;
    public event ChatHeader? ChatUpdated;

    public delegate void DeleteChatHeader(Guid chatId);

    public event DeleteChatHeader? ChatDeleted;

    public delegate void MessageHandler(Message message);

    public event MessageHandler? MessageAdded;

    public delegate void DeleteMessageHandler(Guid messageId);

    public event DeleteMessageHandler? MessageDeleted;

    public delegate void AddContentHandler(Guid chatId, Guid messageId, string content);

    public event AddContentHandler? MessageContentAdded;

    public ChatSocketService() : base(Config.BaseUrl)
    {
        TimeUpdated += OnTimeUpdated;
        Client.OnConnected += (sender, args) => RequestUpdates();
        Client.OnReconnected += (sender, args) => RequestUpdates();

        Subscribe<List<ChatReadModel>>("new_chats", chats =>
        {
            foreach (var chat in chats)
            {
                ChatAdded?.Invoke(Chat.FromReadModel(chat));
            }
        });
        Subscribe<ChatReadModel>("update_chat", chat =>
        {
            ChatUpdated?.Invoke(Chat.FromReadModel(chat));
        });
        Subscribe<List<Guid>>("delete_chats", chatIds =>
        {
            foreach (var chatId in chatIds)
            {
                ChatDeleted?.Invoke(chatId);
            }
        });
        Subscribe<List<MessageReadModel>>("new_messages", messages =>
        {
            foreach (var message in messages)
            {
                MessageAdded?.Invoke(Message.FromReadModel(message));
            }
        });
        Subscribe<List<Guid>>("delete_messages", messages =>
        {
            foreach (var message in messages)
            {
                MessageDeleted?.Invoke(message);
            }
        });
        Subscribe<MessageAddContentModel>("message_add_content", data =>
        {
            MessageContentAdded?.Invoke(data.chat, data.uuid, data.content);
        });
        Subscribe<UpdatesModel>("updates", data =>
        {
            foreach (var chat in data.new_chats)
            {
                ChatAdded?.Invoke(Chat.FromReadModel(chat));
            }
            foreach (var chat in data.deleted_chats)
            {
                ChatDeleted?.Invoke(Chat.FromReadModel(chat).Id);
            }
            foreach (var message in data.new_messages)
            {
                MessageAdded?.Invoke(Message.FromReadModel(message));
            }
            foreach (var message in data.deleted_messages)
            {
                MessageDeleted?.Invoke(Message.FromReadModel(message).Id);
            }

            _updatesLoaded = true;
            if (_lastRequestTime != null)
                OnTimeUpdated(_lastRequestTime.Value);
        });
    }

    public async Task Connect()
    {
        await Connect(AuthService.Instance.User?.IdToken);
    }

    private bool _updatesLoaded = false;
    private DateTime? _lastRequestTime;

    private void OnTimeUpdated(DateTime time)
    {
        _lastRequestTime = time;
        if (_updatesLoaded)
        {
            SettingsService.Instance.Set($"{AuthService.Instance.User?.Id}-timestamp", time);
            Logger.LogDebug($"Time updated ({time})");
        }
    }

    private async void RequestUpdates()
    {
        _updatesLoaded = false;
        await Emit("request_updates",
            SettingsService.Instance.Get<DateTime>($"{AuthService.Instance.User?.Id}-timestamp"));
    }
    
    public async Task CreateChat()
    {
        await Emit("new_chat");
    }
    
    public async Task DeleteChat(Guid chatId)
    {
        await Emit("delete_chat", chatId);
    }
    
    public async Task UpdateChat(Guid id, ChatUpdateModel chat)
    {
        await Emit("update_chat", id, chat);
    }
    
    public async Task CreateMessage(MessageCreateModel model, bool prompt)
    {
        await Emit("new_message", model, prompt);
    }
    
    public async Task DeleteMessage(Guid messageId)
    {
        await Emit("delete_message", messageId);
    }
}