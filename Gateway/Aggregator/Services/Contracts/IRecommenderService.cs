namespace Aggregator.Services.Contracts;

public interface IRecommenderService
{
    Task<IEnumerable<Guid>> GetRecommendationsForUser(Guid userId, int count);
}