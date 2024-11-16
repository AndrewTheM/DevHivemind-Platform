using BlogPlatform.Shared.Common.Exceptions;
using BlogPlatform.Shared.Common.Filters;
using Posts.DataAccess.Entities;

namespace Posts.DataAccess.Repositories.Contracts;

public interface IPostRepository : IRepository<Post>
{
    Task<IQueryable<Post>> GetNewestPostsAsync();

    Task<IQueryable<Post>> GetFilteredPostsAsync(PostFilter filter);

    Task<IQueryable<Post>> GetTopRatedPostsWithAuthorsAsync(int count);

    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="id"/> is found.
    /// </summary>
    Task<Post> GetPostWithContentAsync(Guid id);

    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="id"/> is found.
    /// </summary>
    Task<Post> GetPostWithTagsAsync(Guid id);

    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="titleIdentifier"/> is found.
    /// </summary>
    Task<Post> GetCompletePostAsync(string titleIdentifier);

    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="id"/> is found.
    /// </summary>
    Task<double> CalculatePostRatingAsync(Guid id);
    Task<IQueryable<Post>> GetManyByIds(IEnumerable<Guid> ids);
}
