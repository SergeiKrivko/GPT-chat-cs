using Core.LocalRepository.Models;
using Core.RemoteRepository.Models;

namespace Core;

public class Reply
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid ReplyTo { get; set; }
    public string Type { get; set; } = "explicit";
    
    public ReplyLocalModel ToLocalModel()
    {
        return new()
        {
            Id = Id,
            MessageId = MessageId,
            ReplyTo = ReplyTo,
            Type = Type
        };
    }

    public static Reply FromLocalModel(ReplyLocalModel model)
    {
        return new()
        {
            Id = model.Id,
            MessageId = model.MessageId,
            ReplyTo = model.ReplyTo,
            Type = model.Type
        };
    }
    
    public static Reply FromReadModel(ReplyReadModel model)
    {
        return new Reply
        {
            Id = model.uuid,
            MessageId = model.message_uuid,
            ReplyTo = model.reply_to,
            Type = model.type
        };
    }
}