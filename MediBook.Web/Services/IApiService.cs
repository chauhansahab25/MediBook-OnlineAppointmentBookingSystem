using System.Text.Json;

namespace MediBook.Web.Services;

public interface IApiService
{
    Task<JsonElement?> GetAsync(
        string serviceUrl, string endpoint);

    Task<JsonElement?> PostAsync(
        string serviceUrl, string endpoint, object payload);

    Task<JsonElement?> PutAsync(
        string serviceUrl, string endpoint,
        object? payload = null);

    Task<bool> DeleteAsync(
        string serviceUrl, string endpoint);
}