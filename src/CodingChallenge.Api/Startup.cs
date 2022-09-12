using CodingChallenge.Api.Controllers;
using CodingChallenge.Application;
using CodingChallenge.Application.TvShow.Services;
using CodingChallenge.Infrastructure;
using CodingChallenge.Infrastructure.Logging;
using CodingChallenge.Infrastructure.Models;
using CodingChallenge.Infrastructure.Services;

namespace CodingChallenge.Api;

public class Startup
{
    public ILogger logger;
    public IConfiguration configuration;
    public IServiceProvider serviceProvider;
    public AWSAppProject awsApplication;

    public Startup()
    {
        configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables().Build();

        awsApplication = new AWSAppProject();
        configuration.GetSection(Constants.APPLICATION_ENVIRONMENT_VAR_PREFIX).Bind(awsApplication);

        logger = SetupLogger();

        var services = new ServiceCollection();
        ConfigureServices(services);
        serviceProvider = services.BuildServiceProvider();
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationBaseDependencies();
        services.AddInfrastructureDependencies(configuration, logger);
        services.AddScoped<IScrapeService, ScrapeService>();
        services.AddScoped<IMessagePublisher, MessagePublisher>();
        services.AddScoped<ITvMazeHttpClient, TvMazeHttpClient>();

        services.AddSingleton(logger);
        services.AddSingleton(awsApplication);

        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

    }

    public ILogger SetupLogger()
    {
        return new CustomLambdaLoggerProvider(new CustomLambdaLoggerConfig()
        {
            LogLevel = LogLevel.Debug,
            InfrastructureProject = awsApplication

        }).CreateLogger(nameof(ShowController));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
    }
}