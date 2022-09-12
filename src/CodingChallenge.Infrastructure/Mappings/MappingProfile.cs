using System.Globalization;
using AutoMapper;
using CodingChallenge.Application.Mappings;
using CodingChallenge.Domain.TvShow;
using CodingChallenge.Domain.TvShow.ValueObjects;
using CodingChallenge.Infrastructure.Persistence.TvMaze.Entities;

namespace CodingChallenge.Infrastructure.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        typeof(MappingProfile).Assembly.ApplyMappingsFromAssembly(this);

        CreateMap<TvShowRecordEntity, TvShowEntity>()
            .ForMember(dest => dest.Id, a => a.MapFrom(o => o.Index))
            .ForMember(dest => dest.Name, a => a.MapFrom(o => o.Show!.name))
            .ForMember(dest => dest.Cast, a => a.MapFrom(o => o.Show!._embedded!.cast));

        CreateMap<TvShowRoot, TvShowEntity>()
            .ForMember(dest => dest.Id, a => a.MapFrom(o => o.id))
            .ForMember(dest => dest.Name, a => a.MapFrom(o => o.name))
            .ForMember(dest => dest.Cast, a => a.MapFrom(o => o._embedded!.cast));

        CreateMap<Cast, CastMember>()
            .ForMember(dest => dest.Id, a => a.MapFrom(o => o.person!.id))
            .ForMember(dest => dest.Name, a => a.MapFrom(o => o.person!.name))
            .ForMember(dest => dest.BirthDate, a =>
                a.MapFrom(o =>
                    string.IsNullOrEmpty(o.person!.birthday)
                        ? (DateTime?) null
                        : DateTime.ParseExact(o.person!.birthday, "yyyy-mm-dd", CultureInfo.InvariantCulture)));
    }
}