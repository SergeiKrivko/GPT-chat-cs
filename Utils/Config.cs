namespace Utils;

public class Config
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("APP_NAME_SUFFIX") == null
        ? "http://176.109.106.249:8151/"
        : "http://localhost:8000";
    public static string AppName { get; } =  $"GPT-chat{Environment.GetEnvironmentVariable("APP_NAME_SUFFIX")}";
}