using SQLite;

namespace Core.LocalRepository.Models;

public class MessageLocalModel
{
    [PrimaryKey] public Guid Id { get; set; }
    [Indexed] public Guid ChatId { get; set; }
    public string CreatedAt { get; set; }
    public string? DeletedAt { get; set; }
    public string Role { get; set; }
    public string Content { get; set; }
    public string? Model { get; set; }
    public double Temperature { get; set; }
}