using Core.LocalRepository.Models;
using SQLite;

namespace Core;

public class Message
{
    [PrimaryKey] public Guid Id { get; set; }
    [Indexed] public Guid ChatId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string Role { get; set; }
    public string Content { get; set; }
    public string? Model { get; set; }
    public double Temperature { get; set; }
    
    public MessageLocalModel ToLocalModel()
    {
        return new MessageLocalModel()
        {
            Id = Id,
            ChatId = ChatId,
            CreatedAt = CreatedAt.ToString("s"),
            DeletedAt = DeletedAt?.ToString("s"),
            Role = Role,
            Content = Content,
            Model = Model,
            Temperature = Temperature
        };
    }

    public static Message FromLocalModel(MessageLocalModel model)
    {
        return new Message
        {
            Id = model.Id,
            ChatId = model.ChatId,
            CreatedAt = DateTime.Parse(model.CreatedAt),
            DeletedAt = model.DeletedAt == null ? null : DateTime.Parse(model.DeletedAt),
            Role = model.Role,
            Content = model.Content,
            Model = model.Model,
            Temperature = model.Temperature,
        };
    }
}