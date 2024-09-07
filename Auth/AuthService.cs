using System.Net.Http.Json;
using System.Text.Json;
using Utils;

namespace Auth;

public class AuthService
{
    private const string FirebaseApiKey = "AIzaSyA8z4fe_VedzuLvLQk9HnQTFnVeJDRdxkc";

    private static AuthService? _instance;

    private HttpClient _client = new HttpClient();
    private User? _user;

    public User? User
    {
        get => _user;
        private set
        {
            if (value != _user)
            {
                _user = value;
                UserChanged?.Invoke(_user);
                SettingsService.Instance.Set("user", _user);
            }
        }
    }

    public static AuthService Instance
    {
        get
        {
            _instance ??= new AuthService();
            return _instance;
        }
    }

    private AuthService()
    {
        User = SettingsService.Instance.Get<User>("user");
    }

    public delegate void UserChangeHandler(User? user);
    public event UserChangeHandler? UserChanged;

    public async Task<User?> SignIn(SignInRequestBody body)
    {
        var requestBody = JsonContent.Create(body);
        
        var response = await _client.PostAsync(
            $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}",
            requestBody
        );

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadFromJsonAsync<SignInResponseBody>();
            if (responseBody != null)
            {
                var user = new User(responseBody.localId);
                user.Email = responseBody.email;
                user.RefreshToken = responseBody.refreshToken;
                user.IdToken = responseBody.idToken;
                User = user;
                return user;
            }
        }

        return null;
    }
}

public class SignInRequestBody
{
    public string email { get; set; }
    public string password { get; set; }
    public bool returnSecureToken { get; set; } = true;
}

public class SignInResponseBody
{
    public string localId { get; set; }
    public string email { get; set; }
    public string idToken { get; set; }
    public string refreshToken { get; set; }
}
