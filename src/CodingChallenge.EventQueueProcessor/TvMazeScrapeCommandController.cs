using CodingChallenge.Application.TvShow.Base;
using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;
using CodingChallenge.Application.TvShow.Commands.Scrape;
using CodingChallenge.Application.TvShow.Queries;
using CodingChallenge.Domain.TvShow;
using MediatR;

namespace CodingChallenge.EventQueueProcessor;

public class TvMazeScrapeCommandController
{
    private readonly ISender _mediator;

    public TvMazeScrapeCommandController(ISender sender)
    {
        _mediator = sender;
    }

    public async Task<List<ScrapeCommandResponseBase>?> ProcessCommandListAsync(List<ScrapeCommandBase> commandList)
    {
        var responseList = new List<ScrapeCommandResponseBase>();
        foreach (var command in commandList)
        {
            var result = await ExecuteTransactionCommandBaseAsync(command);
            if (result != null)
            {
                responseList.Add(result);
            }
        }
        return responseList;
    }

    public async Task<ScrapeCommandResponseBase?> ExecuteTransactionCommandBaseAsync(ScrapeCommandBase command)
    {
        if (command is ScrapeCommand)
        {
            return await ScrapeAsync((command as ScrapeCommand)!);
        }

        return null;
    }

    public async Task<ScrapeCommandResponse> ScrapeAsync(ScrapeCommand scrapeCommand)
    {
        return await _mediator.Send(scrapeCommand);
    }

    public async Task<AddScrapeTaskCommandResponse> AddScrapeTaskAsync(AddScrapeTaskCommand addScrapeTaskCommand)
    {
        return await _mediator.Send(addScrapeTaskCommand);
    }

    public async Task<TvShowEntity?> GetItemByIdAsync(GetItemByIndexQuery query)
    {
        return await _mediator.Send(query);
    }
}
