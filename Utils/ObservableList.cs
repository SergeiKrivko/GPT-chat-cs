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

    public delegate void MoveHandler(T obj, int index);

    public event MoveHandler? ItemMoved;

    public delegate void ClearHandler();

    public event ClearHandler? Cleared;

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

    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    public void Clear()
    {
        _list.Clear();
        Cleared?.Invoke();
    }

    public List<T> ToList()
    {
        return new List<T>(_list);
    }

    public void MoveItem(int index1, int index2)
    {
        var item = _list[index1];
        _list.RemoveAt(index1);
        _list.Insert(index2, item);
        ItemMoved?.Invoke(item, index2);
    }
    
    public void MoveItem(T item, int index2)
    {
        _list.Remove(item);
        _list.Insert(index2, item);
        ItemMoved?.Invoke(item, index2);
    }
}