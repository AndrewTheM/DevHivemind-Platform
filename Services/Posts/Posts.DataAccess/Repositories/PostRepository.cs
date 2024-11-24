using BlogPlatform.Shared.Common.Filters;
using Microsoft.EntityFrameworkCore;
using Posts.DataAccess.Context;
using Posts.DataAccess.Entities;
using Posts.DataAccess.Repositories.Contracts;

namespace Posts.DataAccess.Repositories;

public class PostRepository : EntityRepository<Post>, IPostRepository
{
    public PostRepository(BlogContext context)
        : base(context)
    {
    }

    public Task<IQueryable<Post>> GetNewestPostsAsync()
    {
        var posts = _set.OrderByDescending(p => p.CreatedOn).AsQueryable();
        return Task.FromResult(posts);
    }

    public async Task<IQueryable<Post>> GetFilteredPostsAsync(PostFilter filter)
    {
        var posts = await GetNewestPostsAsync();

        if (filter.Title is not null)
            posts = posts.Where(p => p.Title.Contains(filter.Title));
        
        if (filter.Author is not null)
            posts = posts.Where(p => p.Author == filter.Author);

        if (filter.Year is not null)
            posts = posts.Where(p => p.CreatedOn.Year == filter.Year.Value);
        
        if (filter.Month is not null)
            posts = posts.Where(p => p.CreatedOn.Month == filter.Month.Value);

        if (filter.Day is not null)
            posts = posts.Where(p => p.CreatedOn.Day == filter.Day.Value);

        if (filter.Tag is not null)
            posts = posts.Where(p => p.Tags.Any(t => t.TagName == filter.Tag));

        return posts;
    }

    public Task<IQueryable<Post>> GetManyByIds(IEnumerable<Guid> ids)
    {
        var posts = _set.Where(p => ids.Contains(p.Id));
        return Task.FromResult(posts);
    }

    public Task<IQueryable<Post>> GetTopRatedPostsWithAuthorsAsync(int count)
    {
        // TODO: optimize
        var posts = _set.Include(p => p.Ratings)
            .OrderByDescending(p => p.Ratings.Average(r => r.RatingValue))
            .Take(count);

        return Task.FromResult(posts);
    }

    public Task<Post> GetPostWithContentAsync(Guid id)
    {
        return EnsureEntityResultAsync(() =>
        {
            return _set.Include(p => p.ContentEntity)
                .SingleAsync(p => p.Id == id);
        });
    }

    public Task<Post> GetPostWithTagsAsync(Guid id)
    {
        return EnsureEntityResultAsync(() =>
        {
            return _set.Include(p => p.Tags)
                .SingleAsync(p => p.Id == id);
        });
    }

    public Task<Post> GetCompletePostAsync(string titleIdentifier)
    {
        return EnsureEntityResultAsync(() =>
        {
            return _set.Include(p => p.ContentEntity)
                .Include(p => p.Tags)
                .SingleOrDefaultAsync(p => p.TitleIdentifier == titleIdentifier);
        });
    }

    public async Task<double> CalculatePostRatingAsync(Guid id)
    {
        try
        {
            var post = await GetByIdAsync(id);

            return await _context.Entry(post)
                .Collection(p => p.Ratings)
                .Query()
                .AverageAsync(r => r.RatingValue);
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }
}
