namespace Utils.Sockets;

public class SocketDataModel<T>
{
    public DateTime time { get; set; }
    public T data { get; set; }
}