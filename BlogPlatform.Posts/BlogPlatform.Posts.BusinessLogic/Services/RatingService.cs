﻿using AutoMapper;
using BlogPlatform.Posts.BusinessLogic.DTO.Requests;
using BlogPlatform.Posts.BusinessLogic.DTO.Responses;
using BlogPlatform.Posts.BusinessLogic.Services.Contracts;
using BlogPlatform.Posts.DataAccess.Context.Contracts;
using BlogPlatform.Posts.DataAccess.Entities;

namespace BlogPlatform.Posts.BusinessLogic.Services;

public class RatingService : IRatingService
{
    private readonly IBloggingUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RatingService(IBloggingUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RatingResponse> GetRatingOfPostByUserAsync(Guid postId, Guid userId)
    {
        var rating = await _unitOfWork.Ratings.GetRatingOfPostByUserAsync(postId, userId);
        return _mapper.Map<RatingResponse>(rating);
    }

    public async Task<RatingResponse> CreateRatingAsync(RatingRequest ratingDto)
    {
        var rating = _mapper.Map<Rating>(ratingDto);

        await _unitOfWork.Ratings.CreateAsync(rating);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<RatingResponse>(rating);
    }

    public async Task EditRatingAsync(Guid id, RatingUpdateRequest ratingDto)
    {
        var rating = await _unitOfWork.Ratings.GetByIdAsync(id);
        _mapper.Map(ratingDto, rating);
        await _unitOfWork.CommitAsync();
    }

    public async Task DeleteRatingAsync(Guid id)
    {
        await _unitOfWork.Ratings.DeleteAsync(id);
        await _unitOfWork.CommitAsync();
    }
}
