using Amazon.DynamoDBv2.DataModel;
using AutoMapper;
using CodingChallenge.Application.Interfaces;
using CodingChallenge.Domain.Base;
using CodingChallenge.Infrastructure.Persistence.TvMaze.Entities;
using TvShowRecordEntity = CodingChallenge.Infrastructure.Persistence.TvMaze.Entities.TvShowRecordEntity;

namespace CodingChallenge.Infrastructure.Persistence.TvMaze;

public class TvMazeShowRecordDataModel : AuditableEntity, IMapFrom<TvShowRecordEntity>
{
    [DynamoDBHashKey] public int TVMazeIndex { get; set; }

    [DynamoDBProperty] public TvShowRoot? Show { get; set; } = new();


    public void Mapping(Profile profile)
    {
        profile.CreateMap<TvShowRecordEntity, TvMazeShowRecordDataModel>()
            .ForMember(d => d.TVMazeIndex, opt => opt.MapFrom(s => s.Index))
            .ForMember(d => d.Show, opt => opt.MapFrom(s => s.Show));

        profile.CreateMap<TvMazeShowRecordDataModel, TvShowRecordEntity>()
            .ForMember(d => d.Index, opt => opt.MapFrom(s => s.TVMazeIndex))
            .ForMember(d => d.Show, opt => opt.MapFrom(s => s.Show));
    }
}