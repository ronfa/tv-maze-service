using CodingChallenge.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services, IConfiguration configuration,ILogger logger)
    {
        services.AddAutoMapper(typeof(InfrastructureDependencyInjection).Assembly);
        services.AddDatabaseContext(configuration,logger);
        return services;
    }
}
