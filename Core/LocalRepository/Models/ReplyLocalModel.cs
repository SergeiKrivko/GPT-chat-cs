using SQLite;

namespace Core.LocalRepository.Models;

public class ReplyLocalModel
{
    [PrimaryKey] public Guid Id { get; set; }
    [Indexed] public Guid MessageId { get; set; }
    public Guid ReplyTo { get; set; }
    public string Type { get; set; }
}