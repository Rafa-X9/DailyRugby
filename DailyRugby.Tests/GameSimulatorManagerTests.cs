using DailyRugby.Application.CRUD;
using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Simulators;
using DailyRugby.Application.Utilitaries;
using DailyRugby.Application.Validators;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DailyRugby.Tests;

public class GameSimulatorManagerTests : IAsyncLifetime
{
    private IChampionshipCrudService _champService = null!;
    private ITeamCrudService _teamService = null!;
    private IGameCrudService _gameService = null!;
    private IGameSimulatorManager _simulatorManager = null!;
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
        serviceScopeMock.Setup(temp => temp.ServiceProvider.GetService(It.IsAny<Type>()))
            .Returns(_db);
        
        _simulatorManager = new GameSimulatorManager(serviceProviderMock.Object,
            new InstantTimer());

        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.CloseAsync();
    }

    #region ScheduleGame

    [Fact]
    public async Task ScheduleGame_TimeInThePast_ReturnsInvalid()
    {
        var champ = await SetUpChampionship();
        await SetUpFourTeams(champ.Id, 95);
        var gamesResult = await _gameService.GenerateRounds(champ.Id);
        Assert.True(gamesResult.IsSuccessful);
        var firstGameId = gamesResult.Item.First().Id;

        var scheduleResult = await _simulatorManager
            .ScheduleGameAsync(firstGameId, DateTime.UtcNow.AddMinutes(-1));
        Assert.False(scheduleResult.IsSuccessful);
        Assert.Equal(Errors.Invalid, scheduleResult.Error);
    }

    [Fact]
    public async Task ScheduleGame_NoMatchingGameId_ReturnsNotFound()
    {
        var champ = await SetUpChampionship();
        await SetUpFourTeams(champ.Id, 95);
        var gamesResult = await _gameService.GenerateRounds(champ.Id);
        Assert.True(gamesResult.IsSuccessful);

        var scheduleResult = await _simulatorManager
            .ScheduleGameAsync(Guid.NewGuid(), DateTime.UtcNow.AddHours(1));
        Assert.False(scheduleResult.IsSuccessful);
        Assert.Equal(Errors.NotFound, scheduleResult.Error);
    }

    [Fact]
    public async Task ScheduleGame_Valid_SchedulesGame()
    {
        var champ = await SetUpChampionship();
        await SetUpFourTeams(champ.Id, 95);
        var gamesResult = await _gameService.GenerateRounds(champ.Id);
        Assert.True(gamesResult.IsSuccessful);
        var firstGameId = gamesResult.Item.First().Id;

        var dateTime = DateTime.UtcNow.AddHours(1);
        var scheduleResult = await _simulatorManager
            .ScheduleGameAsync(firstGameId, dateTime);

        Schedule? schedule = await _db.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(temp => temp.GameId == firstGameId);

        Assert.True(scheduleResult.IsSuccessful);
        Assert.NotNull(schedule);
        Assert.Equal(dateTime, schedule.DateTimeUtc);
    }

    #endregion

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