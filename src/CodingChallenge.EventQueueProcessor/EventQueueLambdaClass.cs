using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using CodingChallenge.Application;
using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;
using CodingChallenge.Application.TvShow.Commands.Scrape;
using CodingChallenge.Application.TvShow.Queries;
using CodingChallenge.Application.TvShow.Services;
using CodingChallenge.Infrastructure;
using CodingChallenge.Infrastructure.Logging;
using CodingChallenge.Infrastructure.Models;
using CodingChallenge.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace CodingChallenge.EventQueueProcessor;

public class EventQueueLambdaClass
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly AWSAppProject _awsApplication;
    private readonly TvMazeLambdaRunner? _runner;

    public const int RetryLimit = 20;

    public EventQueueLambdaClass()
    {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables().Build();
        _awsApplication = new AWSAppProject();
        _configuration.GetSection(Constants.APPLICATION_ENVIRONMENT_VAR_PREFIX).Bind(_awsApplication);

        _logger = SetupLogger();

        var services = new ServiceCollection();
        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        _runner = _serviceProvider!.GetService<TvMazeLambdaRunner>();

    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationBaseDependencies();
        services.AddInfrastructureDependencies(_configuration, _logger);
        services.AddSingleton(_logger);
        services.AddSingleton(_awsApplication);

        services.AddTransient<TvMazeScrapeCommandController, TvMazeScrapeCommandController>();
        services.AddTransient<TvMazeLambdaRunner, TvMazeLambdaRunner>();
        services.AddScoped<IScrapeService, ScrapeService>();
        services.AddScoped<IMessagePublisher, MessagePublisher>();
        services.AddScoped<ITvMazeHttpClient, TvMazeHttpClient>();

    }


    public ILogger SetupLogger()
    {
        return new CustomLambdaLoggerProvider(new CustomLambdaLoggerConfig()
        {
            LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
            InfrastructureProject = _awsApplication

        }).CreateLogger(nameof(EventQueueLambdaClass));
    }

    public async Task HandleAsync(SQSEvent evnt, ILambdaContext context)
    {
        _logger.LogInformation($"Handling SQS Event");

        if (evnt.Records == null || !evnt.Records.Any())
        {
            _logger.LogInformation($"No records are found");
            return;
        }

        foreach (var record in evnt.Records)
        {
            try
            {
                _logger.LogInformation($"log debug {record.Body}");

                var taskObject = JsonConvert.DeserializeObject<AddScrapeTaskCommand>(record.Body);
                int startIndex = Convert.ToInt32(taskObject!.StartIndex);
                int endIndex = Convert.ToInt32(taskObject.EndIndex);

                var tasks = new List<Task>();

                for (int i = startIndex; i <= endIndex; i++)
                {
                    _logger.LogInformation($"{i} - sending scrape command for index {i}");
                    tasks.Add(_runner!.SendScrapeCommand(i));
                }

                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    var result = ((Task<ScrapeCommandResponse>)task).Result;

                    _logger.LogInformation($"{result.Index} - async response received");

                    if (!result.IsSuccess && await ShouldScheduleRetry(result, taskObject))
                    {
                        var newOrder = new AddScrapeTaskCommand(
                            result.Index, result.Index, taskObject.Retries + 1);

                        _logger.LogInformation(
                            $"{result.Index} - Try Count -> {taskObject.Retries} - Adding a new task for a failed task. Id -> {result.Index}.");

                        await _runner!.AddScrapeTaskAsync(newOrder);
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogInformation($"error processing queue... Message: {ex.Message}. StackTrace {ex.StackTrace}. exception type -> {ex.GetType()}");
                _logger.LogInformation($"inner exception error processing queue... Message: {ex.InnerException?.Message}. StackTrace {ex.InnerException?.StackTrace}");
                throw;
            }
        }
    }

    private async Task<bool> ShouldScheduleRetry(ScrapeCommandResponse result, AddScrapeTaskCommand taskObject)
    {
        if (result.NotFound)
        {
            _logger.LogInformation($"Show {result.Index} - Not found. No need to try again");
            return false;
        }

        if (taskObject.Retries < RetryLimit)
        {
            var indexResult = await _runner!.GetItemByIdAsync(new GetItemByIndexQuery(result.Index));

            if (indexResult == null)
            {
                _logger.LogInformation($"Show {result.Index} -- Item is not in database, retrying..");
                return true;
            }

            _logger.LogInformation($"{result.Index} -- Item already exists in database");
            return false;
        }
        else
        {
            _logger.LogInformation($"{result.Index} -- Try Count limit exceeded.");
            return false;
        }
    }
}
