using System.Text;
using System.Text.Json;

namespace Starter.Bff.Services;

public interface IApiAuthService
{
    Task<ApiUserResult?> LoginAsync(string email, string password);
    Task<ApiUserResult?> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<ApiUserResult?> GetUserAsync(Guid userId);
}

public record ApiUserResult(Guid Id, string Email, string FirstName, string LastName);

public class ApiAuthService : IApiAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _http;

    public ApiAuthService(HttpClient http) => _http = http;

    public async Task<ApiUserResult?> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsync("/api/auth/login",
            Json(new { email, password }));
        return response.IsSuccessStatusCode ? await Deserialize(response) : null;
    }

    public async Task<ApiUserResult?> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        var response = await _http.PostAsync("/api/auth/register",
            Json(new { email, password, firstName, lastName }));
        return response.IsSuccessStatusCode ? await Deserialize(response) : null;
    }

    public async Task<ApiUserResult?> GetUserAsync(Guid userId)
    {
        var response = await _http.GetAsync($"/api/auth/user/{userId}");
        return response.IsSuccessStatusCode ? await Deserialize(response) : null;
    }

    private static StringContent Json(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static async Task<ApiUserResult?> Deserialize(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiUserResult>(json, JsonOptions);
    }
}
