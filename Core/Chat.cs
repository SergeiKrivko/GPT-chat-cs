using System.Collections.ObjectModel;
using SQLite;

namespace Core;

public class Chat
{
    [PrimaryKey] public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Model { get; set; }
    
    public bool Sync { get; set; }
    [Ignore] public ObservableCollection<Message> Messages { get; } = new();
    [Ignore] public int? LastLoadedMessage { get; set; } = null;
}