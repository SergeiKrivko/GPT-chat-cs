using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Transport;

namespace Utils.Sockets;

public class SocketService
{
    private readonly SocketIOClient.SocketIO _client;

    protected SocketService(string url)
    {
        _client = new SocketIOClient.SocketIO(url, new SocketIOOptions
        {
            Transport = TransportProtocol.WebSocket,
        });
    }

    public delegate void TimeHandler(DateTime time);

    public event TimeHandler? TimeUpdated;

    public async Task Connect(string? token)
    {
        if (token != null)
            _client.Options.Auth = token;
        Console.WriteLine($"Connecting sockets...");
        await _client.ConnectAsync();
        Console.WriteLine("Sockets connected");
    }

    public delegate void Handler<T>(T data);
    public delegate object? HandlerWithCallback<T>(T data);

    public void Subscribe<T>(string key, Handler<T> handler)
    {
        _client.On(key, response =>
        {
            Console.WriteLine($"Socket {key}");
            var data = response.GetValue<SocketDataModel<T>>();
            if (data != null)
            {
                TimeUpdated?.Invoke(data.time);
                handler(data.data);
            }
        });
    }
    
    public void Subscribe<T>(string key, HandlerWithCallback<T> handler)
    {
        _client.On(key, async response =>
        {
            var data = response.GetValue<SocketDataModel<T>>();
            if (data != null)
            {
                TimeUpdated?.Invoke(data.time);
                var callback = handler(data.data);
                await response.CallbackAsync(callback);
            }
        });
    }

    public async Task Emit(string key, params object[] data)
    {
        if (!_client.Connected)
        {
            Console.WriteLine("Not connected");
            return;
        }
        await _client.EmitAsync(key, data);
    }
}