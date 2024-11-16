using BlogPlatform.Shared.Common.Exceptions;
using BlogPlatform.Shared.Common.Filters;
using BlogPlatform.Shared.Common.Pagination;
using Posts.BusinessLogic.DTO.Requests;
using Posts.BusinessLogic.DTO.Responses;

namespace Posts.BusinessLogic.Services.Contracts;

public interface IPostService
{
    /// <summary>
    /// Returns a page of posts determined by <paramref name="filter"/>.
    /// </summary>
    Task<Page<PostResponse>> GetPageOfPostsAsync(PostFilter filter = null);

    Task<IEnumerable<PostResponse>> GetTrendingPostsAsync(int count);

    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="id"/> is found.
    /// </summary>
    Task<PostResponse> FindPostAsync(Guid id);
    
    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="titleIdentifier"/> is found.
    /// </summary>
    Task<CompletePostResponse> GetCompletePostAsync(string titleIdentifier);

    Task<PostResponse> PublishPostAsync(PostRequest post, Guid userId, string username);

    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="id"/> is found.
    /// </summary>
    Task EditPostAsync(Guid id, PostRequest post);

    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="id"/> is found.
    /// </summary>
    Task DeletePostAsync(Guid id);

    /// <summary>
    /// Adds a tag to post for each name in <paramref name="tagNames"/>.
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="id"/> is found.
    /// </summary>
    Task SetTagsOfPostAsync(Guid id, params string[] tagNames);

    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no post with given <paramref name="id"/> is found.
    /// </summary>
    Task<bool> CheckIsPostAuthorAsync(Guid id, Guid userId);
    Task<IEnumerable<PostResponse>> FindManyPostsAsync(IEnumerable<Guid> ids);
}
