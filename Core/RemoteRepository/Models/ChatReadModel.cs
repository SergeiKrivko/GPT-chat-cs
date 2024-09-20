namespace Core.RemoteRepository.Models;

public class ChatReadModel
{
    public Guid uuid { get; set; }
    public DateTime created_at { get; set; }
    public DateTime? deleted_at { get; set; }
    public string name { get; set; }
    public string? model { get; set; }
    public int context_size { get; set; }
    public decimal temperature { get; set; }
    public int? color { get; set; }
}