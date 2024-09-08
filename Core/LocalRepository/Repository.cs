using System.Linq.Expressions;
using SQLite;

namespace Core.LocalRepository;

public class Repository<T> where T: new()
{
    private SQLiteAsyncConnection _database;

    public Repository(SQLiteAsyncConnection database)
    {
        _database = database;
        Init();
    }

    private async void Init()
    {
        await _database.CreateTableAsync<T>();
    }

    public async Task<List<T>> GetAll()
    {
        var lst = await _database.Table<T>().ToListAsync();
        if (lst == null)
            throw new Exception("Not Found");
        return lst;
    }
    
    public async Task<List<T>> GetAll(Expression<Func<T, bool>> predExpr)
    {
        var lst = await _database.Table<T>().Where(predExpr).ToListAsync();
        if (lst == null)
            throw new Exception("Not Found");
        return lst;
    }

    public async Task<T> Get(Expression<Func<T, bool>> predExpr)
    {
        var res = await _database.Table<T>().Where(predExpr).FirstAsync();
        if (res == null)
        {
            throw new Exception("Not Found");
        }

        return res;
    }

    public async Task Insert(T item)
    {
        await _database.InsertOrReplaceAsync(item);
    }
    
    public async Task Save(T item)
    {
        await _database.UpdateAsync(item);
    }
    
    public async Task Remove(T item)
    {
        await _database.DeleteAsync(item);
    }
}