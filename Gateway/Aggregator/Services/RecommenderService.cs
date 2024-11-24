using Aggregator.Services.Contracts;

namespace Aggregator.Services;

public class RecommenderService : IRecommenderService
{
    private readonly HttpClient _httpClient;

    public RecommenderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<IEnumerable<Guid>> GetRecommendationsForUser(Guid userId, int count)
    {
        return _httpClient.GetFromJsonAsync<IEnumerable<Guid>>($"recommend?user_id={userId}&top_k={count}");
    }
}
