using System.Net.Http.Json;
using Utils;
using Utils.Http;
using Utils.Http.Exceptions;

namespace Auth;

public class AuthService : HttpService
{
    private const string FirebaseApiKey = "AIzaSyA8z4fe_VedzuLvLQk9HnQTFnVeJDRdxkc";

    private static AuthService? _instance;

    private HttpClient _client = new HttpClient();
    private User? _user;

    public bool Refreshed { get; private set; } = false;

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
        // Unauthorized += RefreshToken;
        RefreshTokenLoop();
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
                user.ExpiresAt = DateTime.Now + TimeSpan.FromSeconds(responseBody.expiresIn);
                User = user;
                return user;
            }
        }

        return null;
    }

    private async void RefreshTokenLoop()
    {
        while (true)
        {
            if (User == null || User.RefreshToken == null)
            {
                Console.WriteLine("Token refresh failed: log out");
                User = null;
                await Task.Delay(100000);
                continue;
            }
            if (User.ExpiresAt != null)
            {
                await Task.Delay(int.Max(0, (int)(User.ExpiresAt - DateTime.Now).Value.TotalMilliseconds - 10));
            }
            try
            {
                await RefreshToken();
            }
            catch (ConnectionException)
            {
                Console.WriteLine("Token refresh failed: no connection");
                await Task.Delay(10000);
            }
            catch (HttpServiceException)
            {
                Console.WriteLine("Token refresh failed: log out");
                User = null;
                await Task.Delay(100000);
            }
        }
    }

    private async Task RefreshToken()
    {
        Refreshed = false;
        if (User == null)
            return;
        var resp = await Post<RefreshResponseBody>(
            $"https://securetoken.googleapis.com/v1/token?key={FirebaseApiKey}",
            new RefreshRequestBody
            {
                grant_type = "refresh_token",
                refresh_token = User.RefreshToken ?? "",
            });
        User.IdToken = resp.id_token;
        User.RefreshToken = resp.refresh_token;
        User.ExpiresAt = DateTime.Now + TimeSpan.FromSeconds(resp.expires_in);
        User = User;
        Console.WriteLine("Token refreshed success");
        Refreshed = true;
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
    public int expiresIn { get; set; }
}

public class RefreshRequestBody
{
    public string grant_type { get; set; }
    public string refresh_token { get; set; }
}

public class RefreshResponseBody
{
    public string id_token { get; set; }
    public string refresh_token { get; set; }
    public int expires_in { get; set; }
}