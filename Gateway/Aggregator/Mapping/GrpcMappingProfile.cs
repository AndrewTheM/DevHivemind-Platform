using Aggregator.DTO;
using AutoMapper;
using BlogPlatform.Shared.Common.Pagination;
using Google.Protobuf.WellKnownTypes;
using Protos = BlogPlatform.Shared.GRPC.Protos;

namespace Aggregator.Mapping;

public class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        CreateMap<Guid, Protos.Guid>()
            .ConvertUsing(src => new Protos.Guid { Value = src.ToString() });

        CreateMap<Protos.Guid, Guid>()
            .ConvertUsing(src => Guid.Parse(src.Value));

        CreateMap<Timestamp, DateTime>()
            .ConvertUsing(src => src.ToDateTime());

        CreateMap<Protos.CommentModel, CommentDto>();
        CreateMap<Protos.CommentPageResponse, Page<CommentDto>>();
        
        CreateMap<Protos.PostModel, PostDto>();
        CreateMap<Protos.CompletePostResponse, CompletePostDto>();
    }
}
