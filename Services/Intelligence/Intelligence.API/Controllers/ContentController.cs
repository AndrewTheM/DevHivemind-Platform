using Intelligence.API.Constants;
using Intelligence.API.Exceptions;
using Intelligence.API.Models;
using Intelligence.API.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intelligence.API.Controllers;

[Route("api/content-moderation")]
[ApiController]
[Authorize]
public class ContentController : ControllerBase
{
    private readonly IContentService _contentService;

    public ContentController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpPost("post")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ContentResponse>> AnalyzePost(
        [FromBody] PostAnalysisRequest request)
    {
        try
        {
            await _contentService.CheckTextContentAsync(request.Title, "Post Title");
            await _contentService.CheckTextContentAsync(request.Content, "Post Content");
            await _contentService.CheckTextContentAsync(request.TagsString, "Post Tags");
            await _contentService.CheckImageContentAsync(request.ImageString, "Post Thumbnail");
            return Ok(Responses.NoProblemsFound);
        }
        catch (ContentNotAllowedException ex)
        {
            return Ok(Responses.ModerationProblem(ex.Message));
        }
        catch (HttpRequestException)
        {
            return Ok(Responses.ServiceUnavailable);
        }
    }
    
    [HttpPost("comment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ContentResponse>> AnalyzeComment(
        [FromBody] CommentAnalysisRequest request)
    {
        try
        {
            await _contentService.CheckTextContentAsync(request.CommentText, "Comment");
            return Ok(Responses.NoProblemsFound);
        }
        catch (ContentNotAllowedException ex)
        {
            return Ok(Responses.ModerationProblem(ex.Message));
        }
        catch (HttpRequestException)
        {
            return Ok(Responses.ServiceUnavailable);
        }
    }
}
