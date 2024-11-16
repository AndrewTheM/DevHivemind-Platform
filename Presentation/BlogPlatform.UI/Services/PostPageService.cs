using BlogPlatform.UI.Helpers.Contracts;
using BlogPlatform.UI.Models;
using BlogPlatform.UI.Services.Contracts;

namespace BlogPlatform.UI.Services;

public class PostPageService : IPostPageService
{
    private readonly IApiClient _apiClient;

    public PostPageService(IApiClient apiClient, HttpClient httpClient)
    {
        _apiClient = apiClient;
        _apiClient.HttpClient = httpClient;
    }

    public Task<Post> GetPostPageAsync(string titleIdentifier)
    {
        return _apiClient.SendGetApiRequestAsync<Post>($"postpage/{titleIdentifier}");
    }

    public Task<List<Post>> GetRecommendedPosts()
    {
        return _apiClient.SendGetApiRequestAsync<List<Post>>($"postpage/recommend");
    }
}
