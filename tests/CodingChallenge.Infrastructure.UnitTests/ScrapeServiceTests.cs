using System.Net;
using Amazon.SimpleNotificationService.Model;
using AutoFixture;
using AutoMapper;
using CodingChallenge.Application.Response;
using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;
using CodingChallenge.Application.TvShow.Services;
using CodingChallenge.Infrastructure.Mappings;
using CodingChallenge.Infrastructure.Persistence;
using CodingChallenge.Infrastructure.Persistence.TvMaze.Entities;
using CodingChallenge.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using RestSharp;
using Xunit;

namespace CodingChallenge.Infrastructure.UnitTests;

public class ScrapeServiceTests
{
    private readonly Fixture _fixture;
    private readonly ITvMazeHttpClient _restClient;
    private readonly ITvMazeDatabaseContext _context;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMapper _mapper;


    private IScrapeService _service;

    public ScrapeServiceTests()
    {
        _fixture = new();
        _restClient = Substitute.For<ITvMazeHttpClient>();
        _context = Substitute.For<ITvMazeDatabaseContext>();
        _messagePublisher = Substitute.For<IMessagePublisher>();
        var configuration = new MapperConfiguration(config =>
            config.AddProfile<MappingProfile>());

        _mapper = configuration.CreateMapper();

        _service = new ScrapeService(_mapper, Substitute.For<ILogger<ScrapeService>>(), _context, _messagePublisher,
            _restClient);
    }

    #region Add Scrape Task

    [Fact]
    public async Task AddScrapeTaskAsync()
    {
        var request = _fixture.Create<AddScrapeTaskCommand>();

        _messagePublisher.PublishAsync(Arg.Any<PublishRequest>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await _service.AddScrapeTaskAsync(request, CancellationToken.None);

        response.Should().BeTrue();

        await _messagePublisher
            .Received(1)
            .PublishAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Scrape 

    [Fact]
    public async Task ScrapeIsSuccessful_WhenItemAlreadyExistsInDatabase()
    {
        var request = _fixture.Create<int>();
        var data = new TvShowRecordEntity {Show = GetRoot(), Index = 1};

        _context.GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(data);

        var response = await _service.ScrapeAsync(request, CancellationToken.None);

        response.Should().BeOfType<TvShowDataResponse>();
        response.IsSuccessful.Should().BeTrue();
        response.AlreadyStored.Should().BeTrue();

        await _context
            .Received(1)
            .GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScrapeFail_WhenNullResponseReceived()
    {
        var request = _fixture.Create<int>();

        _context.GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        _restClient.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsNullForAnyArgs();

        var response = await _service.ScrapeAsync(request, CancellationToken.None);

        response.Should().BeOfType<TvShowDataResponse>();
        response.IsSuccessful.Should().BeFalse();

        await _context
            .Received(1)
            .GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());

        await _restClient
            .Received(1)
            .GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScrapeFail_WhenTooManyRequests()
    {
        var request = _fixture.Create<int>();

        _context.GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        _restClient.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new RestResponse { StatusCode = HttpStatusCode.TooManyRequests });

        var response = await _service.ScrapeAsync(request, CancellationToken.None);

        response.Should().BeOfType<TvShowDataResponse>();
        response.IsSuccessful.Should().BeFalse();
        response.RateLimited.Should().BeTrue();

        await _context
            .Received(1)
            .GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());

        await _restClient
            .Received(1)
            .GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScrapeFail_WhenShowNotFound()
    {
        var request = _fixture.Create<int>();

        _context.GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        _restClient.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new RestResponse { StatusCode = HttpStatusCode.NotFound });

        var response = await _service.ScrapeAsync(request, CancellationToken.None);

        response.Should().BeOfType<TvShowDataResponse>();
        response.IsSuccessful.Should().BeFalse();
        response.NotFound.Should().BeTrue();

        await _context
            .Received(1)
            .GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());

        await _restClient
            .Received(1)
            .GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScrapeSucceeds()
    {
        var request = _fixture.Create<int>();
        var data = GetRoot();

        _context.GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        _restClient.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new RestResponse
            { StatusCode = HttpStatusCode.OK, IsSuccessful = true, Content = JsonConvert.SerializeObject(data) });

        var response = await _service.ScrapeAsync(request, CancellationToken.None);

        response.Should().BeOfType<TvShowDataResponse>();
        response.IsSuccessful.Should().BeTrue();
        response.TvShow.Should().NotBeNull();

        await _context
            .Received(1)
            .GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());

        await _restClient
            .Received(1)
            .GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());

        await _context
            .Received(1)
            .SaveAsync(Arg.Any<int>(), Arg.Any<TvShowDataResponse>(), Arg.Any<CancellationToken>());
    }


    #endregion

    private TvShowRoot GetRoot()
    {
        var data = _fixture.Create<TvShowRoot>();
        var castMembers = data._embedded!.cast!.Where(c => !string.IsNullOrEmpty(c.person!.birthday)).Select(c =>
        {
            c.person!.birthday = DateTime.UtcNow.ToString("yyyy-mm-dd");
            return c;
        });
        data._embedded.cast = castMembers.ToList();
        return data;
    }
}

