using Aggregator.DTO;
using Aggregator.Extensions;
using Aggregator.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace Aggregator.Controllers;

[Route("api/postpage")]
[ApiController]
[Authorize(Roles = "Reader")]
public class PostPageController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IRecommenderService _recommenderService;
    private readonly IDistributedCache _cache;

    public PostPageController(IPostService postService, ICommentService commentService,
        IRecommenderService recommenderService, IDistributedCache cache)
    {
        _postService = postService;
        _commentService = commentService;
        _recommenderService = recommenderService;
        _cache = cache;
    }

    [HttpGet("{titleIdentifier}")]
    [AllowAnonymous]
    public async Task<ActionResult<CompletePostDto>> GetPostPage(
        [FromRoute] string titleIdentifier)
    {
        var post = await _postService.GetCompletePostAsync(titleIdentifier);

        try
        {
            post.CommentPage = await _commentService.GetPageOfPostCommentsAsync(post.Id);
        }
        catch
        {
            post.CommentPage = new();
        }

        return Ok(post);
    }

    [HttpGet("recommend")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetRecommendedPostsForUser([FromQuery] int count = 10)
    {
        var userId = HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        (Guid userGuid, string cacheKey) = userId switch
        {
            null => (Guid.NewGuid(), "recommend_default"),
            _ => (Guid.Parse(userId), $"recommend_{userId}")
        };

        var posts = await _cache.GetAsync<IEnumerable<PostDto>>(cacheKey);
        if (posts is null)
        {
            var postIds = await _recommenderService.GetRecommendationsForUser(userGuid, count);
            posts = await _postService.GetManyPostsAsync(postIds);
            await _cache.SetAsync(cacheKey, posts, options: new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(20),
            });
        }

        return Ok(posts);
    }
}
