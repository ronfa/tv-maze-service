using CodingChallenge.Application;
using CodingChallenge.Application.TvShow.Services;
using CodingChallenge.Infrastructure;
using CodingChallenge.Infrastructure.Models;
using CodingChallenge.Infrastructure.Services;
using CommandLine;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Console;

public class Program
{
    private static IConfiguration? _configuration;
    private static ILogger? _logger;
    private static ServiceProvider? _serviceProvider;
    public static AWSAppProject? AwsApplication;

    private static async Task Main(string[] args)
    {
        LoadConfiguration();
        SetupLogger();
        ConfigureServices(new ServiceCollection());
        var commandParser = _serviceProvider!.GetService<TvMazeConsoleRunner>();
        var parser = Parser.Default.ParseArguments<CommandLineOptions>(args);

        await parser.WithParsedAsync(async options => await commandParser!.RunOptionsAsync(options));
        await parser.WithNotParsedAsync(async errs => await commandParser!.HandleParseErrorAsync(errs));
    }

    private static void LoadConfiguration()
    {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables().Build();
        AwsApplication = new AWSAppProject();
        _configuration.GetSection(Constants.APPLICATION_ENVIRONMENT_VAR_PREFIX).Bind(AwsApplication);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationBaseDependencies();
        services.AddInfrastructureDependencies(_configuration!, _logger!);
        //services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
        services.AddSingleton(_logger!);
        services.AddTransient<TvMazeScrapeCommandController, TvMazeScrapeCommandController>();
        services.AddTransient<TvMazeConsoleRunner, TvMazeConsoleRunner>();
        services.AddScoped<ITvMazeHttpClient, TvMazeHttpClient>();
        services.AddScoped<IScrapeService, ScrapeService>();
        services.AddScoped<IMessagePublisher, MessagePublisher>();

        services.AddSingleton(AwsApplication!);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void SetupLogger()
    {
        var loggerFactory = LoggerFactory.Create(
            builder => builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information));

        _logger = loggerFactory.CreateLogger("CodingChallenge Console App");
    }
}