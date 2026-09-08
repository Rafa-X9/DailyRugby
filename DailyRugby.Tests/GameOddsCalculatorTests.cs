using DailyRugby.Application.Calculators;
using DailyRugby.Application.CRUD;
using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Simulators;
using DailyRugby.Application.Validators;
using DailyRugby.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace DailyRugby.Tests;

public class GameOddsCalculatorTests(ITestOutputHelper output) : IAsyncLifetime
{
    private IGameOddsCalculator _oddsCalculator = null!;
    private IChampionshipCrudService _champService = null!;
    private ITeamCrudService _teamService = null!;
    private IGameCrudService _gameService = null!;
    private ITestOutputHelper _output = output;
    private AppDbContext _db = null!;
    private SqliteConnection _connection = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);

        _champService = new ChampionshipCrudService(_db);
        _teamService = new TeamCrudService(_db, new TeamValidatorFactory());
        _gameService = new GameCrudService(_db);

        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceScopeMock = new Mock<IServiceScope>();

        serviceProviderMock.Setup(temp => temp.GetService(typeof(AppDbContext)))
            .Returns(_db);

        serviceProviderMock.Setup(temp => temp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        scopeFactoryMock.Setup(temp => temp.CreateScope())
            .Returns(serviceScopeMock.Object);

        serviceScopeMock.Setup(temp => temp.ServiceProvider.GetService(typeof(AppDbContext)))
            .Returns(_db);

        serviceProviderMock.Setup(temp => temp.GetService(typeof(IGameSimulatorFactory)))
            .Returns(new GameSimulatorFactory());

        _oddsCalculator = new SeasonOneOddsCalculator(serviceProviderMock.Object);

        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.CloseAsync();
    }

    [Fact]
    public async Task OddsCalculator_Test()
    {
        var champ = await SetUpChampionship();
        await SetUpThreeTeams(champ.Id, 95);
        await _gameService.GenerateRounds(champ.Id);

        var game = await _db.Games.FirstAsync();

        _output.WriteLine($"The drawn game was:\n" +
            $"Team A: {game.Teams[0].Team.Country}\n" +
            $"- Insight: {game.Teams[0].Team.Insight}\n" +
            $"- Physique: {game.Teams[0].Team.Physique}\n" +
            $"- Technique: {game.Teams[0].Team.Technique}\n" +
            $"Team B: {game.Teams[1].Team.Country}\n" +
            $"- Insight: {game.Teams[1].Team.Insight}\n" +
            $"- Physique: {game.Teams[1].Team.Physique}\n" +
            $"- Technique: {game.Teams[1].Team.Technique}\n");

        await _oddsCalculator.CalculateAsync(game.Id);

        var result = await _db
            .GameOdds
            .FirstAsync();

        _output.WriteLine($"Total simulations: {result.TotalSimulations}\n" +
            $"Team A wins: {result.TeamAWins}\n" +
            $"Team B wins: {result.TeamBWins}\n" +
            $"Tie: {result.Tie}");
    }

    #region Helpers

    private async Task<ChampionshipResponse> SetUpChampionship(Seasons season = Seasons.Season1)
    {
        ChampionshipAddRequest request = new("Champ", season);
        var result = await _champService.AddAsync(request);
        if (!result.IsSuccessful) throw new Exception();
        return result.Item;
    }

    private async Task<(TeamResponse teamA, TeamResponse teamB, TeamResponse teamC)>
        SetUpThreeTeams(Guid champId, int startBudget)
    {
        return (await SetUpTeamA(champId, startBudget),
            await SetUpTeamB(champId, startBudget),
            await SetUpTeamC(champId, startBudget));
    }

    private async Task<(TeamResponse teamA, TeamResponse teamB, TeamResponse teamC, TeamResponse teamD)>
        SetUpFourTeams(Guid champId, int startBudget)
    {
        return (await SetUpTeamA(champId, startBudget),
            await SetUpTeamB(champId, startBudget),
            await SetUpTeamC(champId, startBudget),
            await SetUpTeamD(champId, startBudget));
    }

    private async Task<TeamResponse> SetUpTeamA(Guid champId, int statBudget)
    {
        TeamAddRequest request = new(champId,
            "RafaX9",
            "Brazil",
            statBudget - 2, 1, 1,
            Coaches.General);
        var result = await _teamService.AddAsync(request);
        if (!result.IsSuccessful) throw new Exception();
        return result.Item;
    }

    private async Task<TeamResponse> SetUpTeamB(Guid champId, int statBudget)
    {
        TeamAddRequest request = new(champId,
            "Onko342",
            "Taiwan",
            10, statBudget - 20, 10,
            Coaches.General);
        var result = await _teamService.AddAsync(request);
        if (!result.IsSuccessful) throw new Exception();
        return result.Item;
    }

    private async Task<TeamResponse> SetUpTeamC(Guid champId, int statBudget)
    {
        TeamAddRequest request = new(champId,
            "DonutDaniel5",
            "SovietUnion",
            10, 10, statBudget - 20,
            Coaches.General);
        var result = await _teamService.AddAsync(request);
        if (!result.IsSuccessful) throw new Exception();
        return result.Item;
    }

    private async Task<TeamResponse> SetUpTeamD(Guid champId, int statBudget)
    {
        TeamAddRequest request = new(champId,
            "ChelseaFanForever",
            "Singapore",
            5, statBudget - 10, 5,
            Coaches.General);
        var result = await _teamService.AddAsync(request);
        if (!result.IsSuccessful) throw new Exception();
        return result.Item;
    }

    private async Task<GameResponse?> GetGameBetween(Guid teamA, Guid teamB, IList<GameResponse> list)
    {
        return list.FirstOrDefault(temp =>
            (temp.TeamA.Team.Id == teamA &&
            temp.TeamB.Team.Id == teamB)
            ||
            (temp.TeamA.Team.Id == teamB &&
            temp.TeamB.Team.Id == teamA));
    }

    #endregion
}