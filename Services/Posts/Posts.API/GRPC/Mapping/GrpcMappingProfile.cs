using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Posts.BusinessLogic.DTO.Responses;
using Protos = BlogPlatform.Shared.GRPC.Protos;

namespace Posts.API.GRPC.Mapping;

public class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        CreateMap<Guid, Protos.Guid>()
            .ConvertUsing(src => new Protos.Guid { Value = src.ToString() });
        
        CreateMap<Protos.Guid, Guid>()
            .ConvertUsing(src => Guid.Parse(src.Value));

        CreateMap<DateTime, Timestamp>()
            .ConvertUsing(src => Timestamp.FromDateTime(DateTime.SpecifyKind(src, DateTimeKind.Utc)));

        CreateMap<CompletePostResponse, Protos.CompletePostResponse>()
            .ForMember(dest => dest.Author, opts => opts.NullSubstitute(string.Empty))
            .ForMember(dest => dest.ThumbnailPath, opts => opts.NullSubstitute(string.Empty))
            .ForMember(dest => dest.AudioPath, opts => opts.NullSubstitute(string.Empty));

        CreateMap<PostResponse, Protos.PostModel>()
            .ForMember(dest => dest.Author, opts => opts.NullSubstitute(string.Empty))
            .ForMember(dest => dest.ThumbnailPath, opts => opts.NullSubstitute(string.Empty));
    }
}
