using CodingChallenge.Application.Exceptions;
using CodingChallenge.Application.TvShow.Base;
using CodingChallenge.Application.TvShow.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Application.TvShow.Commands.Scrape;

public record ScrapeCommand(int Index) : ScrapeCommandBase, IRequest<ScrapeCommandResponse>;

public record ScrapeCommandResponse(int Index) : ScrapeCommandResponseBase
{
    public bool NotFound { get; set; }
    public bool RateLimited { get; internal set; }
}

public class ScrapeCommandHandler : IRequestHandler<ScrapeCommand, ScrapeCommandResponse>
{
    private readonly IScrapeService _scrapeService;
    private readonly ILogger _logger;

    public ScrapeCommandHandler(IScrapeService scrapeService, ILogger logger)
    {
        _scrapeService = scrapeService;
        _logger = logger;
    }

    public async Task<ScrapeCommandResponse> Handle(ScrapeCommand request, CancellationToken cancellationToken)
    {
        var retRec = new ScrapeCommandResponse(request.Index);

        try
        {
            var result = await _scrapeService.ScrapeAsync(request.Index, cancellationToken);

            if (!result.IsSuccessful)
            {
                retRec.ErrorMessage = "error occured.";
            }

            if (result.NotFound)
            {
                retRec.NotFound = result.NotFound;
                retRec.ErrorMessage = "not found.";
            }

            if (result.RateLimited)
            {
                retRec.RateLimited = result.RateLimited;
                retRec.ErrorMessage = "rate limited.";
            }
        }
        catch (ItemAlreadyExistsException ex)
        {
            retRec.ErrorMessage = ex.Message;
        } catch(Exception ex){
            _logger.LogWarning($"exception handling scrap command. {ex.Message} -- {ex.InnerException} -- {ex.StackTrace}");    
            throw;
        }

        return retRec;
    }
}
