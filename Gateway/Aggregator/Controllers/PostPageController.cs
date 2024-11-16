using Aggregator.DTO;
using Aggregator.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aggregator.Controllers
{
    [Route("api/postpage")]
    [ApiController]
    [Authorize(Roles = "Reader")]
    public class PostPageController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IRecommenderService _recommenderService;

        public PostPageController(IPostService postService, ICommentService commentService,
            IRecommenderService recommenderService)
        {
            _postService = postService;
            _commentService = commentService;
            _recommenderService = recommenderService;
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
            var userGuid = userId is null ? Guid.NewGuid() : Guid.Parse(userId);
            var postIds = await _recommenderService.GetRecommendationsForUser(userGuid, count);
            var posts = await _postService.GetManyPostsAsync(postIds);
            return Ok(posts);
        }
    }
}
