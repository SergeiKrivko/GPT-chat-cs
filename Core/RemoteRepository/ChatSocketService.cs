using Auth;
using Utils;
using Utils.Sockets;

namespace Core.RemoteRepository;

public class ChatSocketService: SocketService
{
    private static ChatSocketService? _instance;

    public static ChatSocketService Instance
    {
        get
        {
            _instance ??= new ChatSocketService();
            return _instance;
        }
    }
    
    public ChatSocketService() : base("http://localhost:8000/")
    {
        TimeUpdated += time => SettingsService.Instance.Set($"{AuthService.Instance.User?.Id}-timestamp", time);
    }

    public async Task Connect()
    {
        await Connect(AuthService.Instance.User?.IdToken);
    }
}