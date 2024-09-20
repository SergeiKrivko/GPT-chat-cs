using System.Collections.ObjectModel;
using Core.LocalRepository.Models;
using Core.RemoteRepository.Models;
using SQLite;
using Utils;

namespace Core;

public class Chat : IComparable<Chat>
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string Name { get; set; } = "";
    public string? Model { get; set; }
    public int ContextSize { get; set; }
    public decimal Temperature { get; set; }
    public int? Color { get; set; }
    
    public ObservableList<Message> Messages { get; } = new();
    public Guid? LastLoadedMessage { get; set; } = null;
    
    public Message? LastMessage { get; private set; }

    private Chat()
    {
        Messages.ItemInserted += (index, message) => LastMessage = Messages.Count == 0 ? null : Messages[^1];
        Messages.ItemRemoved += (index, message) => LastMessage = Messages.Count == 0 ? null : Messages[^1];
    }

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
            Temperature = Temperature,
            Color = Color,
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
            Color = model.Color,
        };
    }
    
    public static Chat FromReadModel(ChatReadModel model)
    {
        return new Chat
        {
            Id = model.uuid,
            CreatedAt = model.created_at,
            DeletedAt = model.deleted_at,
            Name = model.name,
            Model = model.model,
            ContextSize = model.context_size,
            Temperature = model.temperature,
            Color = model.color,
        };
    }

    public delegate void UpdateHandler();

    public event UpdateHandler? Updated;

    public void Update(Chat chat)
    {
        Id = chat.Id;
        CreatedAt = chat.CreatedAt;
        DeletedAt = chat.DeletedAt;
        Name = chat.Name;
        Model = chat.Model;
        ContextSize = chat.ContextSize;
        Temperature = chat.Temperature;
        Updated?.Invoke();
    }

    public Message GetMessage(Guid id)
    {
        foreach (var message in Messages)
        {
            if (message.Id == id)
                return message;
        }

        throw new KeyNotFoundException("Message not found");
    }

    public int CompareTo(Chat? other)
    {
        if (LastMessage?.CreatedAt == null)
        {
            if (other?.LastMessage?.CreatedAt == null)
                return 0;
            return -1;
        }
        if (other?.LastMessage?.CreatedAt == null)
            return 1;
        return LastMessage?.CreatedAt > other.LastMessage?.CreatedAt ? 1 : -1;
    }
}