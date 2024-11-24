using Intelligence.API.Constants;
using Intelligence.API.Exceptions;
using Intelligence.API.Models.Azure;
using Intelligence.API.Services.Contracts;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Intelligence.API.Services;

public class ContentService : IContentService
{
    const int contentRequestLimit = 1000;

    private readonly HttpClient _httpClient;

    public ContentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CheckTextContentAsync(string content, string contentSource)
    {
        for (var i = 0; i < content.Length; i += contentRequestLimit)
        {
            var chunkLength = Math.Min(content.Length - i, contentRequestLimit);
            var chunk = content.Substring(i, chunkLength);
            var request = new ContentSafetyTextRequest(chunk);
            await CheckContentAsync(request, "text", contentSource);
        }
    }

    public Task CheckImageContentAsync(string content, string contentSource)
    {
        if (string.IsNullOrEmpty(content))
            return Task.CompletedTask;

        var request = new ContentSafetyImageRequest(content);
        return CheckContentAsync(request, "image", contentSource);
    }

    private async Task CheckContentAsync(object requestBody, string contentType,
        string contentSource = "Content")
    {
        var json = JsonSerializer.Serialize(requestBody);

        var httpMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new($"{_httpClient.BaseAddress}/{contentType}:analyze?api-version={ClientConsts.ContentSafetyApiVersion}"),
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        var response = await _httpClient.SendAsync(httpMessage);
        response.EnsureSuccessStatusCode();

        var contentResponse = await response.Content.ReadFromJsonAsync<ContentSafetyResponse>();
        var categories = contentResponse.CategoryResults
            .Where(cr => cr.Severity > 0)
            .Select(cr => cr.Category)
            .ToList();
        
        if (categories.Any())
            throw new ContentNotAllowedException(contentSource, string.Join(", ", categories));
    }
}
