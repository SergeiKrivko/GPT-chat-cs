namespace Core.RemoteRepository.Models;

public class ReplyCreateModel
{
    public Guid reply_to { get; set; }
    public string type { get; set; }
}