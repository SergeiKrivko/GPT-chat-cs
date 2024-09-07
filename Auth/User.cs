namespace Auth;

public class User
{
    public string Id { get; set; }
    public string? Email { get; set; }
    public string? IdToken { get; set; }
    public string? RefreshToken { get; set; }

    public User(string id)
    {
        Id = id;
    }
}