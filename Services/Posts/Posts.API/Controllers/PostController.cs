using BlogPlatform.Shared.Common.Filters;
using BlogPlatform.Shared.Common.Pagination;
using BlogPlatform.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Posts.API.Extensions;
using Posts.BusinessLogic.DTO.Requests;
using Posts.BusinessLogic.DTO.Responses;
using Posts.BusinessLogic.Services.Contracts;
using System.Security.Claims;

namespace Posts.API.Controllers;

[Route("api/posts")]
[ApiController]
[Authorize(Roles = "Admin, Author")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publishEndpoint;

    public PostController(
        IPostService postService, IDistributedCache cache, IPublishEndpoint publishEndpoint)
    {
        _postService = postService;
        _cache = cache;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Page<PostResponse>>> GetPosts([FromQuery] PostFilter filter)
    {
        string cacheKey = $"posts_{filter.GetHashCode()}";
        var posts = await _cache.GetAsync<Page<PostResponse>>(cacheKey);

        if (posts is null)
        {
            posts = await _postService.GetPageOfPostsAsync(filter);
            await _cache.SetAsync(cacheKey, posts, options: new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2),
            });
        }

        return Ok(posts);
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetTrendingPosts([FromQuery] int top = 5)
    {
        if (top is < 1 or > 20)
            return BadRequest();

        string cacheKey = $"trending_{top}";
        var posts = await _cache.GetAsync<IEnumerable<PostResponse>>(cacheKey);

        if (posts is null)
        {
            posts = await _postService.GetTrendingPostsAsync(top);
            await _cache.SetAsync(cacheKey, posts, options: new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2),
            });
        }

        return Ok(posts);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostResponse>> GetPostById([FromRoute] Guid id)
    {
        string cacheKey = $"post_{id}";
        var post = await _cache.GetAsync<PostResponse>(cacheKey);

        if (post is null)
        {
            post = await _postService.FindPostAsync(id);
            await _cache.SetAsync(cacheKey, post, options: new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(30),
            });
        }

        return Ok(post);
    }

    [HttpGet("complete/{titleIdentifier}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompletePostResponse>> GetCompletePostById(
        [FromRoute] string titleIdentifier)
    {
        var post = await _cache.GetAsync<CompletePostResponse>(titleIdentifier);

        if (post is null)
        {
            post = await _postService.GetCompletePostAsync(titleIdentifier);
            await _cache.SetAsync(titleIdentifier, post, options: new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                SlidingExpiration = TimeSpan.FromSeconds(30),
            });
        }

        return Ok(post);
    }

    [HttpPost("many")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PostResponse>>> ManyPosts([FromBody] IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        if (idList.Count > 10)
            return BadRequest();

        var posts = await _postService.FindManyPostsAsync(idList);
        return Ok(posts);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PostResponse>> CreatePost([FromBody] PostRequest postDto)
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = HttpContext.User.FindFirstValue(ClaimTypes.Name);
        var post = await _postService.PublishPostAsync(postDto, Guid.Parse(userId), username);
        return Ok(post);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePost(
        [FromRoute] Guid id, [FromBody] PostRequest postDto)
    {
        bool userIsPermitted = await CheckIsAuthorOfPostOrAdminAsync(id);

        if (!userIsPermitted)
            return Forbid();

        await _postService.EditPostAsync(id, postDto);
        await _cache.RemoveAsync($"post_{id}");
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePost([FromRoute] Guid id)
    {
        bool userIsPermitted = await CheckIsAuthorOfPostOrAdminAsync(id);

        if (!userIsPermitted)
            return Forbid();

        await _publishEndpoint.Publish<PostDeletionEvent>(new() { PostId = id });

        await _postService.DeletePostAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetTagsOfPost(
        [FromRoute] Guid id, [FromBody] PostTagsRequest tagsRequest)
    {
        bool userIsPermitted = await CheckIsAuthorOfPostOrAdminAsync(id);

        if (!userIsPermitted)
            return Forbid();

        await _postService.SetTagsOfPostAsync(id, tagsRequest.Tags);
        return NoContent();
    }

    private async Task<bool> CheckIsAuthorOfPostOrAdminAsync(Guid id)
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _postService.CheckIsPostAuthorAsync(id, Guid.Parse(userId))
            || HttpContext.User.IsInRole("Admin");
    }
}
