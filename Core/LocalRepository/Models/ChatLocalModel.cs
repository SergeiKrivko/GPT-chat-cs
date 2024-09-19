using SQLite;

namespace Core.LocalRepository.Models;

public class ChatLocalModel
{
    [PrimaryKey] public Guid Id { get; set; }
    public string CreatedAt { get; set; }
    public string? DeletedAt { get; set; }
    public string Name { get; set; }
    public string? Model { get; set; }
    public int ContextSize { get; set; }
    public decimal Temperature { get; set; }
}