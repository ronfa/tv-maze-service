using System.Runtime.Serialization;
using AutoMapper;
using CodingChallenge.Infrastructure.Mappings;
using CodingChallenge.Infrastructure.Persistence.TvMaze;
using CodingChallenge.Infrastructure.Persistence.TvMaze.Entities;
using Xunit;

namespace CodingChallenge.Infrastructure.UnitTests.Mappings;

public class MappingTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        _configuration = new MapperConfiguration(config =>
            config.AddProfile<MappingProfile>());

        _mapper = _configuration.CreateMapper();
    }

    [Theory]
    [InlineData(typeof(TvShowRecordEntity), typeof(TvMazeShowRecordDataModel))]
    [InlineData(typeof(TvMazeShowRecordDataModel), typeof(TvShowRecordEntity))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);
        _mapper.Map(instance, source, destination);
    }

    private object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return FormatterServices.GetUninitializedObject(type);
    }
}
