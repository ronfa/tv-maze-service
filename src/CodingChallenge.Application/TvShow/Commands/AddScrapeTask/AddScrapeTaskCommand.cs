using CodingChallenge.Application.TvShow.Base;
using CodingChallenge.Application.TvShow.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Application.TvShow.Commands.AddScrapeTask;

public record AddScrapeTaskCommand(int StartIndex, int EndIndex, int Retries) : ScrapeCommandBase(), 
    IRequest<AddScrapeTaskCommandResponse>;

public record AddScrapeTaskCommandResponse(int StartIndex, int EndIndex) : ScrapeCommandResponseBase();

public class AddScrapeTaskCommandHandler : IRequestHandler<AddScrapeTaskCommand, AddScrapeTaskCommandResponse>
{
    private readonly IScrapeService _scrapeService;
    private readonly ILogger _logger;

    public AddScrapeTaskCommandHandler(IScrapeService scrapeService, ILogger logger)
    {
        _scrapeService = scrapeService;
        _logger = logger;
    }

    public async Task<AddScrapeTaskCommandResponse> Handle(AddScrapeTaskCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddScrapeTaskCommand command handler called");

        await _scrapeService.AddScrapeTaskAsync(request, cancellationToken);
        var response = new AddScrapeTaskCommandResponse(request.StartIndex,request.EndIndex);
        return response;
    }

}
