using AutoMapper;
using CodingChallenge.Application.Interfaces;
using CodingChallenge.Domain.Interfaces;
using CodingChallenge.Domain.TvShow;
using CodingChallenge.Infrastructure.Persistence;
using CodingChallenge.Infrastructure.Persistence.TvMaze.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CodingChallenge.Infrastructure.Repository.TvMaze;

public class TvMazeRepository : IReadOnlyTvMazeRepository, ITvMazeRepository
{
    private readonly ITvMazeDatabaseContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public TvMazeRepository(ITvMazeDatabaseContext context, IMapper mapper, ILogger logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TvShowEntity?> GetByIndexAsync(int index, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Get by index {index}  repo action is being executed...");

        var result = await _context.GetByIndexAsync(index, cancellationToken);

        if (result == null)
        {
            _logger.LogWarning($"Get By index {index} found no results");
            return null;
        }

        _logger.LogInformation(
            $"{index} - GetByIndexAsync success with Id {index}. result received: {JsonConvert.SerializeObject(result)}");

        var mappedEntity = _mapper.Map<TvShowRecordEntity, TvShowEntity>(result);

        if (mappedEntity == null || mappedEntity.Id == 0)
        {
            _logger.LogWarning($"Get By index {index} mapping error ");
            return null;
        }

        return mappedEntity;
    }

    public async Task<Tuple<List<TvShowEntity>, string>> GetItemListAsync(int pageSize, string? paginationToken = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"page size: {pageSize} paginationToken: {paginationToken}");

        var result = await _context.GetItemListAsync(pageSize, paginationToken, cancellationToken);

        var mappedEntity = _mapper.Map<List<TvShowRecordEntity>, List<TvShowEntity>>(result.Item1);

        var sorted = mappedEntity.Select(s =>
            new TvShowEntity
            {
                Id = s.Id, 
                Name = s.Name,
                Cast = s.Cast.OrderBy(c => c.BirthDate.HasValue).ThenByDescending(c => c.BirthDate)
            }
        ).ToList();

        return new Tuple<List<TvShowEntity>, string>(sorted, result.Item2);
    }
}