using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MediBook.Web.Services;

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    // ── GET ───────────────────────────────────────────────────────────────────

    public async Task<JsonElement?> GetAsync(
        string serviceUrl, string endpoint)
    {
        try
        {
            var client = CreateClient(serviceUrl);
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content
                .ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<JsonElement>(
                json, JsonOptions());
        }
        catch
        {
            return null;
        }
    }

    // ── POST ──────────────────────────────────────────────────────────────────

    public async Task<JsonElement?> PostAsync(
        string serviceUrl, string endpoint, object payload)
    {
        try
        {
            var client = CreateClient(serviceUrl);
            var content = Serialize(payload);
            var response = await client.PostAsync(
                endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content
                .ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<JsonElement>(
                json, JsonOptions());
        }
        catch
        {
            return null;
        }
    }

    // ── PUT ───────────────────────────────────────────────────────────────────

    public async Task<JsonElement?> PutAsync(
        string serviceUrl, string endpoint,
        object? payload = null)
    {
        try
        {
            var client = CreateClient(serviceUrl);
            var content = payload != null
                ? Serialize(payload)
                : null;

            var response = await client.PutAsync(
                endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content
                .ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<JsonElement>(
                json, JsonOptions());
        }
        catch
        {
            return null;
        }
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    public async Task<bool> DeleteAsync(
        string serviceUrl, string endpoint)
    {
        try
        {
            var client = CreateClient(serviceUrl);
            var response = await client.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private HttpClient CreateClient(string baseUrl)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);

        var token = _httpContextAccessor
            .HttpContext?.Session.GetString("JwtToken");

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", token);
        }

        return client;
    }

    private StringContent Serialize(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return new StringContent(
            json, Encoding.UTF8, "application/json");
    }

    private JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}