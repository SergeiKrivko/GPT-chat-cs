namespace Core.RemoteRepository.Models;

public class MessageAddContentModel
{
    public Guid uuid { get; set; }
    public Guid chat { get; set; }
    public string content { get; set; }
}