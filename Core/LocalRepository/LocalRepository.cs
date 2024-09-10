using Auth;
using Core.LocalRepository.Models;
using SQLite;
using Utils;

namespace Core.LocalRepository;

public class LocalRepository
{
    private static LocalRepository? _instance;
    private SQLiteAsyncConnection? _database;
    
    private Repository<ChatLocalModel> Chats { get; set; }
    private Repository<MessageLocalModel> Messages { get; set; }

    public static LocalRepository Instance
    {
        get
        {
            _instance ??= new LocalRepository();
            return _instance;
        }
    }

    public async Task Init()
    {
        if (_database != null)
            await _database.CloseAsync();
        
        var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SergeiKrivko",
            Config.AppName);
        if (AuthService.Instance.User == null)
        {
            path = Path.Join(path, "DefaultUser");
        }
        else
        {
            path = Path.Join(path, "Users", AuthService.Instance.User.Id);
        }
        Directory.CreateDirectory(path);
        path = Path.Join(path, "Database.db");
        _database = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

        Chats = new Repository<ChatLocalModel>(_database);
        Messages = new Repository<MessageLocalModel>(_database);
    }

    public async Task<Chat> GetChat(Guid id)
    {
        return Chat.FromLocalModel(await Chats.Get(t => t.Id == id));
    }

    public async Task<List<Chat>> GetAllChats()
    {
        var res = new List<Chat>();
        foreach (var model in await Chats.GetAll(c => c.DeletedAt == null))
        {
            res.Add(Chat.FromLocalModel(model));
        }

        return res;
    }

    public async Task SaveChat(Chat chat)
    {
        await Chats.Save(chat.ToLocalModel());
    }

    public async Task InsertChat(Chat chat)
    {
        await Chats.Insert(chat.ToLocalModel());
    }

    public async Task RemoveChat(Guid chatId)
    {
        var chat = await Chats.Get(t => t.Id == chatId);
        await Chats.Remove(chat);
    }

    public async Task<Message> GetMessage(Guid id)
    {
        return Message.FromLocalModel(await Messages.Get(t => t.Id == id));
    }

    public async Task<List<Message>> GetAllMessages(Guid chatId)
    {
        var res = new List<Message>();
        foreach (var model in await Messages.GetAll(m => m.ChatId == chatId, t => t.CreatedAt))
        {
            res.Add(Message.FromLocalModel(model));
        }

        return res;
    }

    public async Task SaveMessage(Message message)
    {
        await Messages.Save(message.ToLocalModel());
    }

    public async Task InsertMessage(Message message)
    {
        await Messages.Insert(message.ToLocalModel());
    }

    public async Task RemoveMessage(Guid messageId)
    {
        var message = await Messages.Get(t => t.Id == messageId);
        await Messages.Remove(message);
    }
}