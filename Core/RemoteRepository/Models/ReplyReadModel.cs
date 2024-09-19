namespace Core.RemoteRepository.Models;

public class ReplyReadModel
{
    public Guid uuid { get; set; }
    public Guid message_uuid { get; set; }
    public Guid reply_to { get; set; }
    public string type { get; set; }
}