using Posts.DataAccess.Entities;
using BlogPlatform.Shared.Common.Exceptions;

namespace Posts.DataAccess.Repositories.Contracts;

public interface ITagRepository : IRepository<Tag>
{
    Task<IQueryable<Tag>> GetRelevantTagsAsync();

    /// <summary>
    /// Throws <see cref="EntityNotFoundException"/>
    /// if no tag with given <paramref name="name"/> is found.
    /// </summary>
    Task<Tag> GetTagByNameAsync(string name);
}
