using System.Threading.Tasks;
using Utils;
using Utils.Http;

namespace ErrorHandler;

public class LogHttpService : HttpService
{
    public LogHttpService()
    {
        BaseUrl = Config.BaseUrl;
    }

    public async Task SendLog(string logText)
    {
        await Post("api/v1/logs", new ErrorLogRequestBody
        {
            application = "Avalonia",
            version = Config.Version,
            log = logText,
        });
    }
}

class ErrorLogRequestBody
{
    public string application { get; set; }
    public string version { get; set; }
    public string log { get; set; }
}