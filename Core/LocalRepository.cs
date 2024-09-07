using SQLite;

namespace Core;

public class LocalRepository
{
    private static LocalRepository? _instance;
    private SQLiteAsyncConnection _database;

    private LocalRepository()
    {
        var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SergeiKrivko",
            "GPT-chat", "DefaultUser");
        Directory.CreateDirectory(path);
        path = Path.Join(path, "Database.db");
        _database = new SQLiteAsyncConnection(path, SQLite.SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        _init();
    }

    public static LocalRepository Instance
    {
        get
        {
            _instance ??= new LocalRepository();
            return _instance;
        }
    }

    private async void _init()
    {
        await _database.CreateTableAsync<Chat>();
        await _database.CreateTableAsync<Message>();
    }

    public async Task<List<Chat>> GetAllChats()
    {
        var lst = await _database.Table<Chat>().ToListAsync();
        if (lst == null)
            throw new Exception("Can not get all chats");
        return lst;
    }

    public async Task<Chat> GetChat(Guid id)
    {
        var res = await _database.Table<Chat>().Where(t => t.Id == id).FirstAsync();
        if (res == null)
        {
            throw new KeyNotFoundException("Chat not found!");
        }

        return res;
    }

    public async Task InsertChat(Chat chat)
    {
        await _database.InsertAsync(chat);
    }

    public async Task SaveChat(Chat chat)
    {
        await _database.UpdateAsync(chat);
    }
    
    public async Task<Message> GetMessage(Guid id)
    {
        var res = await _database.Table<Message>().Where(t => t.Id == id).FirstAsync();
        if (res == null)
        {
            throw new KeyNotFoundException("Message not found!");
        }

        return res;
    }
    
    public async Task<Message> GetMessage(Guid chatId, long index)
    {
        var res = await _database.Table<Message>().Where(t => t.ChatId == chatId && t.Index == index).FirstAsync();
        if (res == null)
        {
            throw new KeyNotFoundException("Message not found!");
        }

        return res;
    }

    public async Task<List<Message>> GetAllMessages(Guid? chatId = null, long? minIndex = null, long? maxIndex = null)
    {
        var selector = _database.Table<Message>();
        if (chatId != null)
            selector = selector.Where(t => t.ChatId == chatId);
        if (minIndex != null)
            selector = selector.Where(t => t.Index >= minIndex);
        if (maxIndex != null)
            selector = selector.Where(t => t.Index <= maxIndex);

        selector = selector.OrderBy(t => t.Index);
        
        var lst = await selector.ToListAsync();
        if (lst == null)
            throw new Exception("Can not get all chats");
        return lst;
    }
}