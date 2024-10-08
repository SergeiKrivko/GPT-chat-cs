﻿using Microsoft.Extensions.Logging;
using SocketIOClient;
using SocketIOClient.Transport;

namespace Utils.Sockets;

public class SocketService
{
    protected readonly SocketIOClient.SocketIO Client;

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
        LogService.Logger.Information("Connecting...");
        await Client.ConnectAsync();
        LogService.Logger.Information("Connected");
    }

    public delegate void Handler<T>(T data);
    public delegate object? HandlerWithCallback<T>(T data);

    public void Subscribe<T>(string key, Handler<T> handler)
    {
        Client.On(key, response =>
        {
            var data = response.GetValue<SocketDataModel<T>>();
            if (data != null)
            {
                LogService.Logger.Debug($"Socket '{key}' received");
                TimeUpdated?.Invoke(data.time);
                handler(data.data);
            }
            else
            {
                LogService.Logger.Warning($"Socket '{key}' received with invalid data");
            }
        });
    }
    
    public void Subscribe<T>(string key, HandlerWithCallback<T> handler)
    {
        Client.On(key, async response =>
        {
            var data = response.GetValue<SocketDataModel<T>>();
            if (data != null)
            {
                LogService.Logger.Debug($"Socket '{key}' received");
                TimeUpdated?.Invoke(data.time);
                var callback = handler(data.data);
                await response.CallbackAsync(callback);
            }
            else
            {
                LogService.Logger.Warning($"Socket '{key}' received with invalid data");
            }
        });
    }

    public async Task Emit(string key, params object[] data)
    {
        if (!Client.Connected)
        {
            LogService.Logger.Warning($"Not connected: failed to emit '{key}'");
            return;
        }
        LogService.Logger.Debug($"Socket '{key}' emitted");
        await Client.EmitAsync(key, data);
    }

    public async Task Emit<T>(string key, Handler<T> handler, params object[] data)
    {
        if (!Client.Connected)
        {
            LogService.Logger.Warning($"Not connected: failed to emit '{key}'");
            return;
        }
        LogService.Logger.Debug($"Socket '{key}' emitted");
        await Client.EmitAsync(key, response =>
        {
            LogService.Logger.Debug($"Response to socket '{key}' received");
            var resp = response.GetValue<SocketDataModel<T>>();
            TimeUpdated?.Invoke(resp.time);
            handler(resp.data);
        }, data);
    }
}