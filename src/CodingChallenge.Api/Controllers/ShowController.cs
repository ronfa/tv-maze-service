using AutoMapper;
using CodingChallenge.Api.Models.Responses;
using CodingChallenge.Application.TvShow.Queries;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace CodingChallenge.Api.Controllers;

[ApiController]
[Route("/api/shows")]
public class ShowController : Controller
{
    public static readonly ProblemDetails NotFoundResponse = new()
    {
        Type = "/api/shows/errors/not-found",
        Title = "TMS000",
        Detail = "Show not found",
        Status = StatusCodes.Status404NotFound
    };

    private readonly ISender _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<ShowController> _logger;

    public ShowController(ISender sender, IMapper mapper, ILogger<ShowController> logger)
    {
        _mediator = sender;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ShowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var show = await _mediator.Send(new GetItemByIndexQuery(id), cancellationToken);

        if (show == null)
        {
            _logger.LogWarning("Show with id {0} could not be found", id);
            return NotFound(NotFoundResponse);
        }

        return Ok(_mapper.Map<ShowResponse>(show));
    }

    [HttpGet("getall/{pageSize:int}")]
    [ProducesResponseType(typeof(PagedList<ShowResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListAsync(
        [FromRoute] int pageSize,
        [FromQuery] string? paginationToken=null,
        CancellationToken cancellationToken=default)
    {
        var shows = await _mediator.Send(
            new GetItemListQuery(pageSize, paginationToken), cancellationToken);

        if (shows == null || shows.Items == null || shows.Items.Count == 0)
        {
            _logger.LogWarning($"No shows found for pageSize {0} and pagination token {1}", pageSize, paginationToken);
            return NotFound(NotFoundResponse);
        }

        var mapped = _mapper.Map<PagedList<ShowResponse>>(shows);

        return Ok(mapped);
    }
}