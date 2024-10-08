﻿using System.Transactions;
using Core.LocalRepository.Models;
using Core.RemoteRepository.Models;

namespace Core;

public class Message
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Model { get; set; }
    public double Temperature { get; set; }
    public List<Reply> Reply { get; set; }
    
    public TranslationLocalModel? Transaction { get; set; }

    public string Text => Transaction?.Text ?? Content;
    
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
    
    public static Message FromReadModel(MessageReadModel model)
    {
        var reply = new List<Reply>();
        foreach (var replyReadModel in model.reply)
        {
            reply.Add(Core.Reply.FromReadModel(replyReadModel));
        }
        
        return new Message
        {
            Id = model.uuid,
            ChatId = model.chat_uuid,
            CreatedAt = model.created_at,
            DeletedAt = model.deleted_at,
            Role = model.role,
            Content = model.content,
            Model = model.model,
            Temperature = model.temperature,
            Reply = reply,
        };
    }
    
    public delegate void UpdateHandler();

    public event UpdateHandler? Updated;

    public void AddContent(string content)
    {
        Content += content;
        Updated?.Invoke();
    }
}