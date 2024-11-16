using AutoMapper;
using BlogPlatform.Shared.Common.Exceptions;
using BlogPlatform.Shared.Common.Extensions;
using BlogPlatform.Shared.Common.Filters;
using BlogPlatform.Shared.Common.Pagination;
using BlogPlatform.Shared.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Posts.BusinessLogic.DTO.Requests;
using Posts.BusinessLogic.DTO.Responses;
using Posts.BusinessLogic.Services.Contracts;
using Posts.DataAccess.Context.Contracts;
using Posts.DataAccess.Entities;
using System.Text.RegularExpressions;

namespace Posts.BusinessLogic.Services;

public class PostService : IPostService
{
    private readonly IBloggingUnitOfWork _unitOfWork;
    private readonly IUriService _uriService;
    private readonly ITimeService _timeService;
    private readonly IMapper _mapper;

    public PostService(
        IBloggingUnitOfWork unitOfWork,
        IUriService uriService,
        ITimeService timeService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _uriService = uriService;
        _timeService = timeService;
        _mapper = mapper;
    }

    public async Task<Page<PostResponse>> GetPageOfPostsAsync(PostFilter filter = null)
    {
        Uri GetPageUri(int pageNumber)
        {
            PostFilter anotherFilter = filter.CopyWithDifferentPage(pageNumber);
            return _uriService.GetPostsPageUri(anotherFilter);
        }

        var filteredPosts = await _unitOfWork.Posts.GetFilteredPostsAsync(filter);
        var pagedPosts = await filteredPosts.Paginate(filter).ToListAsync();
        var responseList = _mapper.Map<List<Post>, List<PostResponse>>(pagedPosts);
        responseList.ForEach(AddRelativeTimeToResponse);

        Uri previousPageUri = GetPageUri(filter.PageNumber - 1);
        Uri nextPageUri = GetPageUri(filter.PageNumber + 1);

        return new(responseList, filteredPosts.Count(), filter, previousPageUri, nextPageUri);
    }

    public async Task<IEnumerable<PostResponse>> GetTrendingPostsAsync(int count)
    {
        var posts = await _unitOfWork.Posts.GetTopRatedPostsWithAuthorsAsync(count);
        var postList = await posts.ToListAsync();
        var responseList = _mapper.Map<List<Post>, List<PostResponse>>(postList);

        responseList.ForEach(AddRelativeTimeToResponse);
        return responseList;
    }

    public async Task<PostResponse> FindPostAsync(Guid id)
    {
        var post = await _unitOfWork.Posts.GetByIdAsync(id);
        var response = _mapper.Map<PostResponse>(post);

        AddRelativeTimeToResponse(response);
        return response;
    }

    public async Task<CompletePostResponse> GetCompletePostAsync(string titleIdentifier)
    {
        var completePost = await _unitOfWork.Posts.GetCompletePostAsync(titleIdentifier);
        var response = _mapper.Map<CompletePostResponse>(completePost);
        response.Rating = await _unitOfWork.Posts.CalculatePostRatingAsync(completePost.Id);

        AddRelativeTimeToResponse(response);
        return response;
    }

    public async Task<PostResponse> PublishPostAsync(PostRequest postDto, Guid userId, string username)
    {
        var post = _mapper.Map<Post>(postDto);
        post.AuthorId = userId;
        post.Author = username;
        post.ContentEntity = _mapper.Map<PostContent>(postDto);

        var cutTitle = post.Title.Length switch
        {
            > 200 => post.Title[..200],
            _ => post.Title
        };

        var cutLowerTitle = cutTitle.ToLowerInvariant();
        var wordOnlyString = Regex.Replace(cutLowerTitle, @"[^\w\s]", string.Empty);
        post.TitleIdentifier = Regex.Replace(wordOnlyString, @"\s+", "-");

        await _unitOfWork.Posts.CreateAsync(post);
        await _unitOfWork.CommitAsync();

        var response = _mapper.Map<PostResponse>(post);
        AddRelativeTimeToResponse(response);

        return response;
    }

    public async Task EditPostAsync(Guid id, PostRequest postDto)
    {
        var post = await _unitOfWork.Posts.GetPostWithContentAsync(id);
        _mapper.Map(postDto, post);

        await _unitOfWork.CommitAsync();
    }

    public async Task DeletePostAsync(Guid id)
    {
        await _unitOfWork.Posts.DeleteAsync(id);
        await _unitOfWork.CommitAsync();
    }

    public async Task SetTagsOfPostAsync(Guid id, params string[] tagNames)
    {
        var post = await _unitOfWork.Posts.GetPostWithTagsAsync(id);
        post.Tags = new List<Tag>();

        foreach (string tagName in tagNames)
        {
            Tag tag;

            try
            {
                tag = await _unitOfWork.Tags.GetTagByNameAsync(tagName);
            }
            catch (EntityNotFoundException)
            {
                tag = new() { TagName = tagName };
            }

            if (!post.Tags.Contains(tag))
            {
                post.Tags.Add(tag);
            }
        }

        await _unitOfWork.CommitAsync();
    }

    public async Task<bool> CheckIsPostAuthorAsync(Guid id, Guid userId)
    {
        var post = await _unitOfWork.Posts.GetByIdAsync(id);
        return post.AuthorId == userId;
    }
    
    public async Task<IEnumerable<PostResponse>> FindManyPostsAsync(IEnumerable<Guid> ids)
    {
        var posts = await _unitOfWork.Posts.GetManyByIds(ids);
        var postList = await posts.ToListAsync();
        var responseList = _mapper.Map<List<Post>, List<PostResponse>>(postList);

        responseList.ForEach(AddRelativeTimeToResponse);
        return responseList;
    }

    private void AddRelativeTimeToResponse(PostResponse response)
    {
        response.RelativePublishTime = _timeService.ConvertToLocalRelativeString(response.PublishedOn);
    }
}
