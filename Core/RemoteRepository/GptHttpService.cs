using System.Net.Http.Headers;
using Auth;
using Core.RemoteRepository.Models;
using Utils;
using Utils.Http;
using Utils.Http.Exceptions;

namespace Core.RemoteRepository;

public class GptHttpService: BodyDetailHttpService
{
    private static GptHttpService? _instance;

    public static GptHttpService Instance
    {
        get
        {
            _instance ??= new GptHttpService();
            return _instance;
        }
    }

    private GptHttpService()
    {
        BaseUrl = Config.BaseUrl;
    }

    public async Task<List<string>> GetModels()
    {
        try
        {
            return await Get<List<string>>("/api/v1/gpt/models");
        }
        catch (NotFoundException)
        {
            return new();
        }
    }
}