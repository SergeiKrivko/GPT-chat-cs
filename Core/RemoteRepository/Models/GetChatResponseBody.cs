namespace Core.RemoteRepository.Models;

public class GetChatResponseBody
{
    public Guid uuid { get; set; }
    public DateTime created_at { get; set; }
    public DateTime deleted_at { get; set; }
    public string name { get; set; }
    public string model { get; set; }
    public int context_size { get; set; }
    public double temperature { get; set; }
}