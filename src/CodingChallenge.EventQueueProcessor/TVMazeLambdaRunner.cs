using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;
using CodingChallenge.Application.TvShow.Commands.Scrape;
using CodingChallenge.Application.TvShow.Queries;
using CodingChallenge.Domain.TvShow;

namespace CodingChallenge.EventQueueProcessor;

public class TvMazeLambdaRunner
{
    TvMazeScrapeCommandController _TVMazeRecordCommandHandler;

    public TvMazeLambdaRunner(TvMazeScrapeCommandController handler)
    {
        _TVMazeRecordCommandHandler = handler;
    }

    public async Task<ScrapeCommandResponse> SendScrapeCommand(int index)
    {
        return await _TVMazeRecordCommandHandler.ScrapeAsync(new ScrapeCommand(index));

    }

    public async Task<AddScrapeTaskCommandResponse> AddScrapeTaskAsync(AddScrapeTaskCommand addScrapeTaskCommand)
    {
        return await _TVMazeRecordCommandHandler.AddScrapeTaskAsync(addScrapeTaskCommand);
    }

    public async Task<TvShowEntity?> GetItemByIdAsync(GetItemByIndexQuery query)
    {
        return await _TVMazeRecordCommandHandler.GetItemByIdAsync(query);
    }    
}