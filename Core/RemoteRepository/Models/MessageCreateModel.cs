namespace Core.RemoteRepository.Models;

public class MessageCreateModel
{
    public Guid chat_uuid { get; set; }
    public string role { get; set; }
    public string content { get; set; }
    public string? model { get; set; }
    public decimal temperature { get; set; }
}