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
    private Repository<ReplyLocalModel> Replys { get; set; }
    private Repository<TranslationLocalModel> Translations { get; set; }

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
        Replys = new Repository<ReplyLocalModel>(_database);
        Translations = new Repository<TranslationLocalModel>(_database);
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
        var mes = Message.FromLocalModel(await Messages.Get(t => t.Id == id));
        mes.Reply = await GetReply(id);
        mes.Transaction = await GetTranslation(mes.Id);
        return mes;
    }

    public async Task<List<Message>> GetAllMessages(Guid chatId)
    {
        var res = new List<Message>();
        foreach (var model in await Messages.GetAll(m => m.ChatId == chatId && m.DeletedAt == null, t => t.CreatedAt))
        {
            var mes = Message.FromLocalModel(model);
            mes.Reply = await GetReply(mes.Id);
            mes.Transaction = await GetTranslation(mes.Id);
            res.Add(mes);
        }

        return res;
    }

    public async Task<List<Message>> GetAllMessagesBefore(Guid chatId, Guid messageId)
    {
        // var beforeMessage = await Messages.Get(t => t.Id == messageId);
        Message? beforeMessage = null;
        try
        {
            beforeMessage = await GetMessage(messageId);
        }
        catch (KeyNotFoundException)
        {
        }

        var res = new List<Message>();
        foreach (var model in await Messages.GetAll(m => m.ChatId == chatId && m.DeletedAt == null,
                     t => t.CreatedAt))
        {
            var mes = Message.FromLocalModel(model);
            mes.Reply = await GetReply(mes.Id);
            mes.Transaction = await GetTranslation(mes.Id);
            res.Add(mes);
        }
        
        if (beforeMessage != null)
            res = res.FindAll(m => m.CreatedAt < beforeMessage.CreatedAt);
        return res;
    }

    private async Task<TranslationLocalModel?> GetTranslation(Guid messageId)
    {
        return await Translations.TryGet(m => m.MessageId == messageId);
    }

    private async Task<List<Reply>> GetReply(Guid messageId)
    {
        var replys = await Replys.GetAll(r => r.MessageId == messageId);
        var res = new List<Reply>();
        foreach (var reply in replys)
        {
            res.Add(Reply.FromLocalModel(reply));
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
        foreach (var reply in message.Reply)
        {
            await Replys.Insert(reply.ToLocalModel());
        }
    }

    public async Task RemoveMessage(Guid messageId)
    {
        try
        {
            var message = await Messages.Get(t => t.Id == messageId);
            await Messages.Remove(message);
        }
        catch (KeyNotFoundException e)
        {
        }
    }

    public async Task AddTranslation(Message message, string text, string src, string dst)
    {
        var translation = new TranslationLocalModel()
        {
            MessageId = message.Id,
            Text = text,
            SrcLang = src,
            DstLang = dst,
        };
        await Translations.Insert(translation);
        message.Transaction = translation;
    }

    public async Task RemoveTranslation(Message message)
    {
        if (message.Transaction != null)
        {
            await Translations.Remove(message.Transaction);
            message.Transaction = null;
        }
    }
}