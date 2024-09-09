using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Transport;

namespace Utils.Sockets;

public class SocketService
{
    protected readonly SocketIOClient.SocketIO Client;
    protected readonly ILogger Logger = LogService.CreateLogger("Sockets");

    protected SocketService(string url)
    {
        Client = new SocketIOClient.SocketIO(url, new SocketIOOptions
        {
            Transport = TransportProtocol.WebSocket,
        });
    }

    public delegate void TimeHandler(DateTime time);

    public event TimeHandler? TimeUpdated;

    public async Task Connect(string? token)
    {
        if (Client.Connected)
            await Client.DisconnectAsync();
        if (token != null)
            Client.Options.Auth = token;
        Logger.LogInformation("Connecting...");
        await Client.ConnectAsync();
        Logger.LogInformation("Connected");
    }

    public delegate void Handler<T>(T data);
    public delegate object? HandlerWithCallback<T>(T data);

    public void Subscribe<T>(string key, Handler<T> handler)
    {
        Client.On(key, response =>
        {
            Logger.LogDebug($"Socket {key} received");
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
        Client.On(key, async response =>
        {
            Logger.LogDebug($"Socket '{key}' received");
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
        if (!Client.Connected)
        {
            Logger.LogWarning($"Not connected: failed to emit '{key}'");
            return;
        }
        Logger.LogDebug($"Socket '{key}' emitted");
        await Client.EmitAsync(key, data);
    }
}