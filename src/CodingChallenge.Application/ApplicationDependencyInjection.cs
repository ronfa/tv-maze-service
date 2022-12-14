using System.Reflection;
using CodingChallenge.Application.Behaviours;
using CodingChallenge.Application.TvShow.Commands.Scrape;
using CodingChallenge.Application.TvShow.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CodingChallenge.Application;

public static class ApplicationDependencyInjection
{

    public static IServiceCollection AddApplicationBaseDependencies(this IServiceCollection services)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(ScrapeCommand)));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        
        
        return services;
    }
}

