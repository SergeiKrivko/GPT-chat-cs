namespace Utils;

public class Config
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("APP_NAME_SUFFIX") == null
        ? "https://gptchat-api.nachert.art/"
        : "http://localhost:8000";
    public static string AppName { get; } =  $"GPT-chat{Environment.GetEnvironmentVariable("APP_NAME_SUFFIX")}";
}