using Auth;
using Utils;
using Utils.Sockets;

namespace Core.RemoteRepository;

public class ChatSocketService: SocketService
{
    public ChatSocketService() : base("http://localhost:8000/")
    {
        TimeUpdated += time => SettingsService.Instance.Set($"{AuthService.Instance.User?.Id}-timestamp", time);
    }

    public async Task Connect()
    {
        await Connect(AuthService.Instance.User?.IdToken);
    }
}