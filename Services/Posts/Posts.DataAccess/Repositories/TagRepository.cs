using Microsoft.EntityFrameworkCore;
using Posts.DataAccess.Context;
using Posts.DataAccess.Entities;
using Posts.DataAccess.Repositories.Contracts;

namespace Posts.DataAccess.Repositories;

public class TagRepository : EntityRepository<Tag>, ITagRepository
{
    public TagRepository(BlogContext context)
        : base(context)
    {
    }

    public Task<IQueryable<Tag>> GetRelevantTagsAsync()
    {
        var tags = _set.Where(t => t.Posts.Any());
        return Task.FromResult(tags);
    }

    public async Task<Tag> GetTagByNameAsync(string name)
    {
        return await EnsureEntityResultAsync(() =>
        {
            return _set.SingleAsync(t => t.TagName == name);
        });
    }
}
