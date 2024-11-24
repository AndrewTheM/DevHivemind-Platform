using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Posts.API.Extensions;
using Posts.BusinessLogic.DTO.Requests;
using Posts.BusinessLogic.DTO.Responses;
using Posts.DataAccess.Context.Contracts;
using Posts.DataAccess.Entities;

namespace Posts.API.Controllers;

[Route("api/posts/tags")]
[ApiController]
[Authorize]
public class TagController : ControllerBase
{
    private readonly IBloggingUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public TagController(IBloggingUnitOfWork unitOfWork, IMapper mapper, IDistributedCache cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetTags()
    {
        var cacheKey = "tags";
        var tags = await _cache.GetAsync<List<string>>(cacheKey);

        if (tags is null)
        {
            var relevantTags = await _unitOfWork.Tags.GetRelevantTagsAsync();
            tags = relevantTags.Select(t => t.TagName).ToList();
            await _cache.SetAsync(cacheKey, tags, options: new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(30),
            });
        }

        return Ok(tags);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponse>> GetTagById([FromRoute] Guid id)
    {
        var tag = await _unitOfWork.Tags.GetByIdAsync(id);
        return _mapper.Map<TagResponse>(tag);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, Author")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateTag([FromBody] TagRequest tagDto)
    {
        var tag = _mapper.Map<Tag>(tagDto);
        await _unitOfWork.Tags.CreateAsync(tag);
        await _unitOfWork.CommitAsync();
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, Author")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateTag(
        [FromRoute] Guid id, [FromBody] TagRequest tagDto)
    {
        var tag = await _unitOfWork.Tags.GetByIdAsync(id);
        _mapper.Map(tagDto, tag);
        await _unitOfWork.CommitAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, Author")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTag([FromRoute] Guid id)
    {
        await _unitOfWork.Tags.DeleteAsync(id);
        await _unitOfWork.CommitAsync();
        return NoContent();
    }
}
