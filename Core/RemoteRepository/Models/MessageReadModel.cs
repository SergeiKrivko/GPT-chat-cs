namespace Core.RemoteRepository.Models;

public class MessageReadModel
{
    public Guid uuid { get; set; }
    public Guid chat_uuid { get; set; }
    public DateTime created_at { get; set; }
    public DateTime? deleted_at { get; set; }
    public string role { get; set; }
    public string content { get; set; }
    public string? model { get; set; }
    public double temperature { get; set; }
}