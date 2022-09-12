using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Console;

public class TvMazeScrapeCommandController
{
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public TvMazeScrapeCommandController(ILogger logger, ISender sender)
    {
        _logger = logger;
        _mediator = sender;
    }

    public async Task<AddScrapeTaskCommandResponse> AddScrapeTaskAsync(AddScrapeTaskCommand addScrapeTaskCommand)
    {
        _logger.LogInformation("Add scrape task command is starting");

        return await _mediator.Send(addScrapeTaskCommand);
    }
}
