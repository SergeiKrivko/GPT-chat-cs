using SQLite;

namespace Core;

public class Message
{
    [Indexed]
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    [PrimaryKey, AutoIncrement]
    public Int64 Index { get; set; }
    public string Role { get; set; } = "User";
    public string Content { get; set; } = "";
}