using System.Net;
using AutoMapper;
using CodingChallenge.Application.Response;
using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;
using CodingChallenge.Application.TvShow.Services;
using CodingChallenge.Domain.TvShow;
using CodingChallenge.Infrastructure.Persistence;
using CodingChallenge.Infrastructure.Persistence.TvMaze.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace CodingChallenge.Infrastructure.Services;

public class ScrapeService : IScrapeService
{
    private readonly ILogger _logger;
    private readonly ITvMazeDatabaseContext _context;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITvMazeHttpClient _restClient;
    private readonly IMapper _mapper;

    public ScrapeService(IMapper mapper, ILogger logger, ITvMazeDatabaseContext context, IMessagePublisher messagePublisher,
        ITvMazeHttpClient client)
    {
        _logger = logger;
        _context = context;
        _messagePublisher = messagePublisher;
        _restClient = client;
        _mapper = mapper;

    }

    public async Task<bool> AddScrapeTaskAsync(AddScrapeTaskCommand command, CancellationToken cancellationToken)
    {
        await _messagePublisher.PublishAsync(command, cancellationToken);

        return true;
    }

    private async Task<TvShowDataResponse> ScrapeTvShow(int id, CancellationToken cancellationToken)
    {
        var retObj = new TvShowDataResponse();
        var response = await TvMazeCastByShowIdHttpGetCall(id, cancellationToken);

        if (response == null)
        {
            retObj.IsSuccessful = false;
            _logger.LogInformation($"{id} - response--EXITING. response is null");
            return retObj;
        }

        _logger.LogInformation($"{id} - StatusCode response code is {response.StatusCode}");

        if (!string.IsNullOrEmpty(response.ErrorMessage))
        {
            _logger.LogInformation($"{id} -  error mesage code is {response.ErrorMessage}");
        }

        _logger.LogInformation($"{id} - response json is {JsonConvert.SerializeObject(response)}");

        if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
        {
            _logger.LogInformation($"{id} - response--SUCCESS.  getting tv maze cast by id :{id} - SUCCESS");

            retObj.IsSuccessful = true;
            retObj.TvShow =
                _mapper.Map<TvShowRoot, TvShowEntity>(JsonConvert.DeserializeObject<TvShowRoot>(response.Content!)!);

            return retObj;
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            retObj.IsSuccessful = false;
            retObj.RateLimited = true;
            _logger.LogInformation($"{id} - response--TOOMANY.getting tv maze cast by id :{id} - TOO MANY");
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            retObj.IsSuccessful = false;
            retObj.NotFound = true;
            _logger.LogInformation($"{id} - response--NOTFOUND.");
        }
        else
        {
            retObj.IsSuccessful = false;
            _logger.LogInformation($"{id} - response--ERROR.  getting tv maze cast by id :{id} - ERROR");
        }

        return retObj;
    }

    public async Task<TvShowDataResponse> ScrapeAsync(int index, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting scrape for index {index}, checking if show exists in db");

        var item = await _context.GetByIndexAsync(index, cancellationToken);

        if (item != null)
        {
            _logger.LogInformation($"{index} already exists in database!");

            return new TvShowDataResponse
            {
                IsSuccessful = true,
                AlreadyStored = true,
                TvShow = _mapper.Map<TvShowRoot, TvShowEntity>(item.Show!)
            };
        }

        var result = await ScrapeTvShow(index, cancellationToken);

        if (!result.IsSuccessful)
            return result;

        _logger.LogInformation($"Show {index} --> Saving to db");

        await _context.SaveAsync(index, result, cancellationToken);

        return result;
    }

    private async Task<RestResponse?> TvMazeCastByShowIdHttpGetCall(int id, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"{id} - getting tv maze cast by id");
            return await _restClient.GetAsync(id, cancellationToken);
        }
        catch (WebException ex)
        {
            _logger.LogError($"{id} - web exception error {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"{id} - HttpRequestException -  {ex.Message} - data: {JsonConvert.SerializeObject(ex.Data)}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{id} - generic exception error {ex.Message}. Type {ex.GetType().Name}");
        }

        return null;
    }
}