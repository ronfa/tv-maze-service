using CodingChallenge.Application.Interfaces;
using CodingChallenge.Domain.Interfaces;
using CodingChallenge.Infrastructure.Repository.TvMaze;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Infrastructure.Persistence;

public static class TvMazePersistenceDependencyInjection
{
    public static IServiceCollection AddDatabaseContext(
        this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Adding database context");

        services.AddScoped<ITvMazeDatabaseContext, TvMazeDatabaseContext>();
        services.AddScoped<IReadOnlyTvMazeRepository, TvMazeRepository>();
        services.AddScoped<ITvMazeRepository, TvMazeRepository>();

        return services;
    }
}
