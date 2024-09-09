namespace Core.RemoteRepository.Models;

public class UpdatesModel
{
    public List<ChatReadModel> new_chats { get; set; } = new();
    public List<ChatReadModel> deleted_chats { get; set; } = new();
    public List<ChatReadModel> updated_chats { get; set; } = new();
    public List<MessageReadModel> new_messages { get; set; } = new();
    public List<MessageReadModel> deleted_messages { get; set; } = new();
}