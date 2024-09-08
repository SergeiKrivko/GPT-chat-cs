using Utils.Http;

namespace Core.RemoteRepository.Models;

public class ChatUpdateModel
{
    public string name { get; set; }
    public string? model { get; set; }
    public int context_size { get; set; }
    public double temperature { get; set; }
}