using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using Posts.API.Extensions;
using Posts.BusinessLogic.DTO.Responses;
using Posts.BusinessLogic.Services.Contracts;
using Protos = BlogPlatform.Shared.GRPC.Protos;

namespace Posts.API.GRPC.Services;

public class PostGrpcService : Protos.PostGrpc.PostGrpcBase
{
    private readonly IPostService _postService;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public PostGrpcService(IPostService postService, IMapper mapper, IDistributedCache cache)
    {
        _postService = postService;
        _mapper = mapper;
        _cache = cache;
    }

    public async override Task<Protos.CompletePostResponse> GetCompletePost(
        Protos.CompletePostRequest request, ServerCallContext context)
    {
        var post = await _cache.GetAsync<CompletePostResponse>(request.TitleIdentifier);

        if (post is null)
        {
            post = await _postService.GetCompletePostAsync(request.TitleIdentifier);
            await _cache.SetAsync(request.TitleIdentifier, post, options: new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                SlidingExpiration = TimeSpan.FromSeconds(1),
            });
        }

        var response = _mapper.Map<Protos.CompletePostResponse>(post);
        return response;
    }

    public async override Task<Protos.ManyPostsResponse> GetManyPosts(
        Protos.ManyPostsRequest request, ServerCallContext context)
    {
        var ids = _mapper.Map<IEnumerable<Protos.Guid>, IEnumerable<Guid>>(request.Ids);
        var posts = await _postService.FindManyPostsAsync(ids);
        var postModels = _mapper.Map<IEnumerable<PostResponse>, IEnumerable<Protos.PostModel>>(posts);
        var response = new Protos.ManyPostsResponse { Posts = { postModels } };
        return response;
    }
}