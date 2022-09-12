using System.ComponentModel.DataAnnotations;
using AutoMapper;
using CodingChallenge.Application.Interfaces;
using CodingChallenge.Domain.TvShow;
using CodingChallenge.Domain.TvShow.ValueObjects;

namespace CodingChallenge.Api.Models.Responses;

public class ShowResponse : IMapFrom<TvShowEntity>
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public IEnumerable<CastMemberResponse> Cast { get; set; } = Enumerable.Empty<CastMemberResponse>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<TvShowEntity, ShowResponse>()
            .ForMember(dest => dest.Id, a => a.MapFrom(o => o.Id))
            .ForMember(dest => dest.Name, a => a.MapFrom(o => o.Name))
            .ForMember(dest => dest.Cast, a => a.MapFrom(o => o.Cast));

        profile.CreateMap<ShowResponse, TvShowEntity>()
            .ForMember(dest => dest.Id, a => a.MapFrom(o => o.Id))
            .ForMember(dest => dest.Name, a => a.MapFrom(o => o.Name))
            .ForMember(dest => dest.Cast, a => a.MapFrom(o => o.Cast));
    }
}

public class CastMemberResponse : IMapFrom<CastMember>
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public DateTime? BirthDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CastMember, CastMemberResponse>()
            .ForMember(dest => dest.Id, a => a.MapFrom(o => o.Id))
            .ForMember(dest => dest.Name, a => a.MapFrom(o => o.Name))
            .ForMember(dest => dest.BirthDate, a => a.MapFrom(o => o.BirthDate));

        profile.CreateMap<CastMemberResponse, CastMember>()
            .ForMember(dest => dest.Id, a => a.MapFrom(o => o.Id))
            .ForMember(dest => dest.Name, a => a.MapFrom(o => o.Name))
            .ForMember(dest => dest.BirthDate, a => a.MapFrom(o => o.BirthDate));
    }
}

