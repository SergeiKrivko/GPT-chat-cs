namespace Utils;

public class ObservableList<T>
{
    private List<T> _list;

    public ObservableList()
    {
        _list = new();
    }

    public ObservableList(List<T> list)
    {
        _list = new List<T>(list);
    }

    public delegate void InsertHandler(int index, T obj);

    public event InsertHandler? ItemInserted;

    public delegate void RemoveHandler(int index, T obj);

    public event RemoveHandler? ItemRemoved;

    public int Count
    {
        get => _list.Count;
    }

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        ItemInserted?.Invoke(index, item);
    }

    public void Add(T item)
    {
        Insert(Count, item);
    }

    public void Pop(int index)
    {
        var item = _list[index];
        _list.RemoveAt(index);
        ItemRemoved?.Invoke(index, item);
    }

    public void Remove(T item)
    {
        var index = _list.IndexOf(item);
        _list.RemoveAt(index);
        ItemRemoved?.Invoke(index, item);
    }

    public T this[int index]
    {
        get => _list[index];
        set
        {
            ItemRemoved?.Invoke(index, _list[index]);
            _list[index] = value;
            ItemInserted?.Invoke(index, value);
        }
    }

    public List<T>.Enumerator GetEnumerator()
    {
        return _list.GetEnumerator();
    }
}