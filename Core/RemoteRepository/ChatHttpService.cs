using System.Net.Http.Headers;
using Auth;
using Core.RemoteRepository.Models;
using Utils.Http;

namespace Core.RemoteRepository;

public class ChatHttpService: BodyDetailHttpService
{
    private static ChatHttpService? _instance;

    public static ChatHttpService Instance
    {
        get
        {
            _instance ??= new ChatHttpService();
            return _instance;
        }
    }

    private ChatHttpService()
    {
        BaseUrl = "http://127.0.0.1:8000";
        Token = AuthService.Instance.User?.IdToken;
        AuthService.Instance.UserChanged += user =>
        {
            Token = user?.IdToken;
        };
    }

    public async Task<GetChatResponseBody[]> GetAllChats()
    {
        return await Get<GetChatResponseBody[]>("/api/v1/chats");
    }
}