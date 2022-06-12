﻿using AutoMapper;
using BlogPlatform.Shared.Common.Filters;
using BlogPlatform.Shared.Common.Pagination;
using Comments.BusinessLogic.DTO.Responses;
using Google.Protobuf.WellKnownTypes;
using Protos = BlogPlatform.Shared.GRPC.Protos;

namespace Comments.API.GRPC.Mapping;

public class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        CreateMap<Guid, Protos.Guid>()
            .ConvertUsing(src => new Protos.Guid { Value = src.ToString() });

        CreateMap<DateTime, Timestamp>()
            .ConvertUsing(src => Timestamp.FromDateTime(src));

        CreateMap<Protos.CommentPageRequest, CommentFilter>();

        CreateMap<CommentResponse, Protos.CommentModel>();
        CreateMap<Page<CommentResponse>, Protos.CommentPageResponse>();
    }
}
