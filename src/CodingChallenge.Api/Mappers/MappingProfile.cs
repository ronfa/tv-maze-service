using AutoMapper;
using CodingChallenge.Api.Models.Responses;
using CodingChallenge.Application.Mappings;
using CodingChallenge.Application.TvShow.Queries;
using CodingChallenge.Domain.TvShow;

namespace CodingChallenge.Api.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        typeof(MappingProfile).Assembly.ApplyMappingsFromAssembly(this);

        CreateMap<PagedList<TvShowEntity>, PagedList<ShowResponse>>();
    }
}


