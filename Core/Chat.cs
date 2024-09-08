using System.Collections.ObjectModel;
using Core.LocalRepository.Models;
using SQLite;

namespace Core;

public class Chat
{
    [PrimaryKey] public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string Name { get; set; }
    public string? Model { get; set; }
    public int ContextSize { get; set; }
    public double Temperature { get; set; }
    
    [Ignore] public ObservableCollection<Message> Messages { get; } = new();
    [Ignore] public int? LastLoadedMessage { get; set; } = null;

    public ChatLocalModel ToLocalModel()
    {
        return new ChatLocalModel
        {
            Id = Id,
            CreatedAt = CreatedAt.ToString("s"),
            DeletedAt = DeletedAt?.ToString("s"),
            Name = Name,
            Model = Model,
            ContextSize = ContextSize,
            Temperature = Temperature
        };
    }

    public static Chat FromLocalModel(ChatLocalModel model)
    {
        return new Chat
        {
            Id = model.Id,
            CreatedAt = DateTime.Parse(model.CreatedAt),
            DeletedAt = model.DeletedAt == null ? null : DateTime.Parse(model.DeletedAt),
            Name = model.Name,
            Model = model.Model,
            ContextSize = model.ContextSize,
            Temperature = model.Temperature,
        };
    }
}