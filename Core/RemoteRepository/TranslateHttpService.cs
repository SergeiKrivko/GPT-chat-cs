using System.Net.Http.Headers;
using Auth;
using Core.RemoteRepository.Models;
using Utils;
using Utils.Http;
using Utils.Http.Exceptions;

namespace Core.RemoteRepository;

public class TranslateHttpService : BodyDetailHttpService
{
    private static TranslateHttpService? _instance;

    public static TranslateHttpService Instance
    {
        get
        {
            _instance ??= new TranslateHttpService();
            return _instance;
        }
    }

    private TranslateHttpService()
    {
        BaseUrl = Config.BaseUrl;
    }

    public async Task<string> DetectLang(string text)
    {
        var res = await Post<DetectLangModel>("/api/v1/translate/detect", new TranslateCreateModel()
        {
            text = text
        });
        return res.lang;
    }

    public async Task<TranslateReadModel> Translate(string text, string dst)
    {
        var res = await Post<TranslateReadModel>($"/api/v1/translate/translate?dst={dst}", new TranslateCreateModel()
        {
            text = text
        });
        return res;
    }
}