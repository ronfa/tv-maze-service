using CodingChallenge.Application.Exceptions;
using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Console;

public class TvMazeConsoleRunner
{
    public const int TotalNumberOfRecords = 50;
    public const int ItemPerMessage = 10;
    private readonly TvMazeScrapeCommandController _controller;

    private readonly ILogger _logger;

    public TvMazeConsoleRunner(ILogger logger, TvMazeScrapeCommandController handler)
    {
        _logger = logger;
        _controller = handler;
    }

    public async Task RunOptionsAsync(CommandLineOptions opts)
    {
        try
        {
            await SendInScrapeOrder();
        }
        catch (RequestValidationException ex)
        {
            _logger.LogError("Validation error!");

            if (ex.Errors.Any())
                foreach (var error in ex.Errors)
                    _logger.LogError($"error: {error.Key} - {string.Join("-", error.Value)}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"CodingChallenge Request: Unhandled Exception. Message: {ex.Message}");
        }
    }

    private async Task SendInScrapeOrder()
    {
        _logger.LogInformation("Starting to schedule scrape tasks...");

        var lastId = 0;
        var tasks = new List<Task>();

        _logger.LogInformation("starting to add tasks ");

        for (var i = 1; i <= TotalNumberOfRecords; i++)
            if (i % ItemPerMessage == 0)
            {
                _logger.LogInformation($"{i} - modules ok last id {lastId}, index ");

                tasks.Add(_controller.AddScrapeTaskAsync(
                    new AddScrapeTaskCommand(lastId + 1, i, 0)));

                lastId = i;
            }

        _logger.LogInformation("All tasks scheduled");

        await Task.WhenAll(tasks);

        _logger.LogInformation("Inspecting schedule results");

        foreach (var task in tasks)
        {
            var result = ((Task<AddScrapeTaskCommandResponse>) task).Result;

            _logger.LogInformation($"got the result... {result.StartIndex} - {result.EndIndex}");
        }

        _logger.LogInformation("All done.");
    }

    public async Task HandleParseErrorAsync(IEnumerable<Error> errs)
    {
        // help requested and version requested are built in and can be ignored.

        if (errs.Any(e => e.Tag != ErrorType.HelpRequestedError && e.Tag != ErrorType.VersionRequestedError))
            foreach (var error in errs)
                _logger.LogWarning($"Command line parameter parse error. {error}");

        await Task.CompletedTask;
    }
}