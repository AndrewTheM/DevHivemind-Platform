using BlogPlatform.UI.Models;

namespace BlogPlatform.UI.Services.Contracts;

public interface IPostPageService
{
    Task<Post> GetPostPageAsync(string titleIdentifier);
    Task<List<Post>> GetRecommendedPosts();
}
