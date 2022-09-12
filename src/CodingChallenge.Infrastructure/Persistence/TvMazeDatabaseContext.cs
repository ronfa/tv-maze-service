using CodingChallenge.Infrastructure.Persistence.TvMaze;
using AutoMapper;
using CodingChallenge.Application.Response;
using CodingChallenge.Infrastructure.Models;
using CodingChallenge.Infrastructure.Persistence.Base;
using CodingChallenge.Infrastructure.Persistence.TvMaze.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CodingChallenge.Domain.TvShow;

namespace CodingChallenge.Infrastructure.Persistence;

public interface ITvMazeDatabaseContext
{
    Task<TvShowRecordEntity?> GetByIndexAsync(int index, CancellationToken cancellationToken);

    Task<Tuple<List<TvShowRecordEntity>, string>> GetItemListAsync(int pageSize, string? paginationToken = null, CancellationToken cancellationToken = default);

    Task SaveAsync(int index, TvShowDataResponse response, CancellationToken cancellationToken);
}

public class TvMazeDatabaseContext : ApplicationDynamoDBBase<TvMazeShowRecordDataModel>, ITvMazeDatabaseContext
{
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public TvMazeDatabaseContext(IMapper mapper, ILogger logger, AWSAppProject awsApplication) : base(logger,
        awsApplication)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TvShowRecordEntity?> GetByIndexAsync(int index, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Get by index {index}  repo action is being executed...");

        var result = await GetAsync(index, cancellationToken);

        if (result == null)
        {
            _logger.LogWarning($"Get By index {index} found no results");
            return null;
        }

        _logger.LogInformation(
            $"{index} - Get By index ({index}) success . result received: {JsonConvert.SerializeObject(result)}");

        var mappedEntity = _mapper.Map<TvMazeShowRecordDataModel, TvShowRecordEntity>(result);

        if (mappedEntity == null || mappedEntity.Index == 0)
        {
            _logger.LogWarning($"Get By index {index} mapping error ");
            return null;
        }

        return mappedEntity;
    }

    public async Task<Tuple<List<TvShowRecordEntity>, string>> GetItemListAsync(int pageSize,
        string? paginationToken = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"page size: {pageSize}, paginationToken: {paginationToken}");

        var result = await GetListAsync(pageSize, paginationToken, cancellationToken);
        var mappedEntity = _mapper.Map<List<TvMazeShowRecordDataModel>, List<TvShowRecordEntity>>(result.Item1);

        return new Tuple<List<TvShowRecordEntity>, string>(mappedEntity, result.Item2);
    }

    public async Task SaveAsync(int index, TvShowDataResponse response, CancellationToken cancellationToken)
    {
        // Saving to db
        var entity = new TvShowRecordEntity
        {
            Index = index,

        };
        if (response.TvShow != null)
        {
            entity.Show = _mapper.Map<TvShowEntity, TvShowRoot>(response.TvShow);
        }

        var dataModel = _mapper.Map<TvShowRecordEntity, TvMazeShowRecordDataModel>(entity);

        dataModel.Created = DateTime.UtcNow;

        await SaveAsync(new List<TvMazeShowRecordDataModel> { dataModel }, cancellationToken);
    }
}