using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Utils;
using Utils.Http;
using Utils.Http.Exceptions;
using Timer = System.Timers.Timer;

namespace Auth;

public class AuthService : HttpService
{
    private const string FirebaseApiKey = "AIzaSyA8z4fe_VedzuLvLQk9HnQTFnVeJDRdxkc";

    private static AuthService? _instance;

    private HttpClient _client = new HttpClient();
    private User? _user;
    private ILogger _logger = LogService.CreateLogger("Auth");

    private bool _refreshed = false;
    
    private Timer _timer;

    public bool Refreshed
    {
        get => _refreshed;
        private set
        {
            _refreshed = false;
            if (value && _user != null)
            {
                _refreshed = true;
                UserReady?.Invoke(_user);
            }
        }
    }

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

            if (_user?.ExpiresAt != null)
            {
                _timer.Interval = int.Max(10, (int)(_user.ExpiresAt - DateTime.Now).Value.TotalMilliseconds - 10000);
                _logger.LogInformation($"Waiting for {_timer.Interval} milliseconds");
                _timer.Start();
            } else
                _timer.Stop();
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
        _timer = new Timer(1000);
        _timer.Elapsed += async (sender, args) => await RefreshToken();
        User = SettingsService.Instance.Get<User>("user");
    }

    public delegate void UserChangeHandler(User? user);
    public event UserChangeHandler? UserChanged;
    
    public delegate void UserReadyHandler(User user);
    public event UserReadyHandler? UserReady;

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

    private async Task RefreshToken()
    {
        _timer.Stop();
        try
        {
            Refreshed = false;
            if (User == null)
                throw new HttpServiceException();
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
            _logger.LogInformation("Token was refreshed successfully");
            User = User;
            Refreshed = true;
        }
        catch (ConnectionException)
        {
            Refreshed = false;
            _timer.Interval = 10000;
            _logger.LogError($"Token refresh failed: no connection. Waiting for {_timer.Interval} milliseconds");
            _timer.Start();
        }
        catch (HttpServiceException)
        {
            Refreshed = false;
            _logger.LogError("Token refresh failed: log out");
            User = null;
        }
    }

    public void SignOut()
    {
        User = null;
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