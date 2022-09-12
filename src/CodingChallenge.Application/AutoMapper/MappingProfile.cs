using AutoMapper;
using CodingChallenge.Application.Mappings;

namespace CodingChallenge.Application.AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        typeof(MappingProfile).Assembly.ApplyMappingsFromAssembly(this);
    }
}
