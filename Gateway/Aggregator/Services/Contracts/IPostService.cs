using Aggregator.DTO;

namespace Aggregator.Services.Contracts;

public interface IPostService
{
    Task<CompletePostDto> GetCompletePostAsync(string titleIdentifier);
    Task<IEnumerable<PostDto>> GetManyPostsAsync(IEnumerable<Guid> ids);
}
