using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace avans_1._4_ai_system_integration_api.Services;

public abstract class AbstractAuthService : IDisposable
{
    private readonly HttpClient _httpClient;

    private string? _token;
    private string? _refreshToken;
    private bool _isRefreshingToken;

    // Global credentials
    public abstract string Email { get; }
    public abstract string Password { get; }

    public AbstractAuthService(HttpClient http, string? refreshToken = null)
    {
        _httpClient = http;
        _refreshToken = refreshToken;
        Console.WriteLine($"[AbstractAuthService] Initialized. BaseUrl: {http.BaseAddress}");
    }

    public void SetToken(string token) => _token = token;

    public void SetRefreshToken(string refreshToken) => _refreshToken = refreshToken;

    public Task<IWebRequestResponse> SendGetRequest(string route) =>
        SendRequestWithRetry(HttpMethod.Get, route);

    public Task<IWebRequestResponse> SendDeleteRequest(string route) =>
        SendRequestWithRetry(HttpMethod.Delete, route);

    public Task<IWebRequestResponse> SendPostRequest(string route, string data) =>
        SendRequestWithRetry(HttpMethod.Post, route, data);

    public Task<IWebRequestResponse> SendPutRequest(string route, string data) =>
        SendRequestWithRetry(HttpMethod.Put, route, data);

    private async Task<IWebRequestResponse> SendRequestWithRetry(
        HttpMethod method,
        string route,
        string? data = null)
    {
        Console.WriteLine($"[SendRequestWithRetry] Started - Method: {method}, Route: {route}");

        var response = await SendRequest(method, route, data);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            Console.WriteLine($"[SendRequestWithRetry] Request successful - StatusCode: {response.StatusCode}");
            return response;
        }

        Console.WriteLine($"[SendRequestWithRetry] 401 Unauthorized detected for route: {route}");

        if (!await EnsureAuthenticatedAsync())
        {
            Console.WriteLine($"[SendRequestWithRetry] Authentication failed for route: {route}");
            return new WebRequestError(
                "Authentication failed.",
                HttpStatusCode.Unauthorized);
        }

        Console.WriteLine($"[SendRequestWithRetry] Authentication restored. Retrying request for route: {route}");

        return await SendRequest(method, route, data);
    }

    private async Task<IWebRequestResponse> SendRequest(
        HttpMethod method,
        string route,
        string? data = null)
    {
        try
        {
            Console.WriteLine($"[SendRequest] Creating HTTP request - Method: {method}, Route: {route}");

            using var request = CreateRequest(method, route, data);

            Console.WriteLine($"[SendRequest] Sending HTTP request to: {request.RequestUri}");
            using var response = await _httpClient.SendAsync(request);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[SendRequest] HTTP request successful - StatusCode: {response.StatusCode}, Route: {route}, ContentLength: {responseContent.Length}");

                return new WebRequestData<string>(
                    responseContent,
                    response.StatusCode);
            }

            Console.WriteLine($"[SendRequest] HTTP request failed - StatusCode: {response.StatusCode}, Route: {route}, Response: {responseContent}");

            return new WebRequestError(
                responseContent,
                response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[SendRequest] HTTP request exception - Route: {route}, Message: {ex.Message}");

            return new WebRequestError(
                $"HTTP Error: {ex.Message}",
                HttpStatusCode.InternalServerError);
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"[SendRequest] HTTP request timeout - Route: {route}");

            return new WebRequestError(
                "Request timeout",
                HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SendRequest] Unexpected exception - Route: {route}, Message: {ex.Message}, StackTrace: {ex.StackTrace}");

            return new WebRequestError(
                ex.Message,
                HttpStatusCode.InternalServerError);
        }
    }

    private HttpRequestMessage CreateRequest(
        HttpMethod method,
        string route,
        string? data)
    {
        var url = $"{route.TrimStart('/')}";

        Console.WriteLine($"[CreateRequest] URL: {url}, Method: {method}, HasData: {!string.IsNullOrWhiteSpace(data)}");

        var request = new HttpRequestMessage(method, url);

        if (!string.IsNullOrWhiteSpace(_token))
        {
            Console.WriteLine("[CreateRequest] Adding Bearer token to request");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);
        }
        else
        {
            Console.WriteLine($"[CreateRequest] WARNING: No token available for request to {url}");
        }

        if (!string.IsNullOrWhiteSpace(data) &&
            method != HttpMethod.Get &&
            method != HttpMethod.Delete)
        {
            Console.WriteLine("[CreateRequest] Adding JSON content to request");
            request.Content = new StringContent(
                RemoveIdFromJson(data),
                Encoding.UTF8,
                "application/json");
        }

        return request;
    }

    private async Task<bool> EnsureAuthenticatedAsync()
    {
        Console.WriteLine("[EnsureAuthenticatedAsync] Called");

        if (!string.IsNullOrWhiteSpace(_refreshToken))
        {
            Console.WriteLine("[EnsureAuthenticatedAsync] Attempting token refresh with existing refresh token");

            if (await RefreshTokenAsync())
            {
                Console.WriteLine("[EnsureAuthenticatedAsync] Token refresh successful");
                return true;
            }

            Console.WriteLine("[EnsureAuthenticatedAsync] Token refresh failed. Falling back to login");
        }
        else
        {
            Console.WriteLine("[EnsureAuthenticatedAsync] No refresh token available. Performing login");
        }

        return await LoginAsync();
    }

    private async Task<bool> RefreshTokenAsync()
    {
        Console.WriteLine("[RefreshTokenAsync] Called");

        if (_isRefreshingToken)
        {
            Console.WriteLine("[RefreshTokenAsync] WARNING: Token refresh already in progress");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_refreshToken))
        {
            Console.WriteLine("[RefreshTokenAsync] WARNING: No refresh token available");
            return false;
        }

        _isRefreshingToken = true;

        try
        {
            Console.WriteLine($"[RefreshTokenAsync] Preparing refresh token request to {_httpClient.BaseAddress}/account/refresh");

            var payload = JsonSerializer.Serialize(new
            {
                refreshToken = _refreshToken
            });

            Console.WriteLine("[RefreshTokenAsync] Refresh token payload prepared");

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_httpClient.BaseAddress}/account/refresh")
            {
                Content = new StringContent(
                    payload,
                    Encoding.UTF8,
                    "application/json")
            };

            Console.WriteLine("[RefreshTokenAsync] Sending refresh token request");
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[RefreshTokenAsync] ERROR: Token refresh failed with StatusCode: {response.StatusCode}");
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("[RefreshTokenAsync] Parsing refresh token response");
            var auth = JsonSerializer.Deserialize<AuthResponse>(
                json,
                JsonOptions);

            if (string.IsNullOrWhiteSpace(auth?.AccessToken))
            {
                Console.WriteLine("[RefreshTokenAsync] ERROR: Refresh response does not contain valid AccessToken");
                return false;
            }

            _token = auth.AccessToken;

            if (!string.IsNullOrWhiteSpace(auth.RefreshToken))
            {
                _refreshToken = auth.RefreshToken;
                Console.WriteLine("[RefreshTokenAsync] New refresh token obtained");
            }

            Console.WriteLine("[RefreshTokenAsync] SUCCESS: Token refresh successful");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RefreshTokenAsync] ERROR: Exception: {ex.Message}");
            return false;
        }
        finally
        {
            _isRefreshingToken = false;
        }
    }

    private async Task<bool> LoginAsync()
    {
        Console.WriteLine("[LoginAsync] Called");

        try
        {
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                Console.WriteLine("[LoginAsync] ERROR: Email or password is missing");
                return false;
            }

            Console.WriteLine($"[LoginAsync] Attempting login with email: {Email}");

            var payload = JsonSerializer.Serialize(new
            {
                email = Email,
                password = Password
            });

            Console.WriteLine("[LoginAsync] Login payload prepared");

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_httpClient.BaseAddress}/account/login")
            {
                Content = new StringContent(
                    payload,
                    Encoding.UTF8,
                    "application/json")
            };

            Console.WriteLine($"[LoginAsync] Sending login request to {_httpClient.BaseAddress}/account/login");
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[LoginAsync] ERROR: Login failed with StatusCode: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[LoginAsync] Response: {responseContent}");
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("[LoginAsync] Parsing login response");
            var auth = JsonSerializer.Deserialize<AuthResponse>(
                json,
                JsonOptions);

            if (string.IsNullOrWhiteSpace(auth?.AccessToken))
            {
                Console.WriteLine("[LoginAsync] ERROR: Login response does not contain valid AccessToken");
                return false;
            }

            _token = auth.AccessToken;
            _refreshToken = auth.RefreshToken;

            var hasRefreshToken = !string.IsNullOrWhiteSpace(_refreshToken);
            Console.WriteLine($"[LoginAsync] SUCCESS: Login successful. AccessToken obtained. RefreshToken available: {hasRefreshToken}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoginAsync] ERROR: Exception: {ex.Message}");
            return false;
        }
    }

    private static string RemoveIdFromJson(string json) =>
        json.Replace("\"id\":\"\",", "")
            .Replace("\"Id\":\"\",", "");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void Dispose() => _httpClient.Dispose();

    // Nested types to keep everything in one file

    public interface IWebRequestResponse
    {
        HttpStatusCode StatusCode { get; }
    }

    public sealed class WebRequestData<T> : IWebRequestResponse
    {
        public T Data { get; }

        public HttpStatusCode StatusCode { get; }

        public WebRequestData(T data, HttpStatusCode statusCode)
        {
            Data = data;
            StatusCode = statusCode;
        }
    }

    public sealed class WebRequestError : IWebRequestResponse
    {
        public string Error { get; }

        public HttpStatusCode StatusCode { get; }

        public WebRequestError(
            string error,
            HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            Error = error;
            StatusCode = statusCode;
        }
    }

    private sealed class AuthResponse
    {
        public string? TokenType { get; set; }

        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }
    }
}