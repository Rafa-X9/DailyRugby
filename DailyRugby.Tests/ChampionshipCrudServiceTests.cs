using DailyRugby.Application.CRUD;
using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Simulators;
using DailyRugby.Application.Validators;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Tests;

public class ChampionshipCrudServiceTests : IAsyncLifetime
{
    private IChampionshipCrudService _champService = null!;
    private ITeamCrudService _teamService = null!;
    private IGameCrudService _gameService = null!;
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

        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.CloseAsync();
    }

    #region Add

    [Fact]
    public async Task Add_NullArgument_ReturnsNullArgument()
    {
        var result = await _champService.AddAsync(null);
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NullArgument, result.Error);
    }

    [Fact]
    public async Task Add_EmptyName_ReturnsInvalid()
    {
        ChampionshipAddRequest request = new("   ", Seasons.Season1);
        var result = await _champService.AddAsync(request);
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.Invalid, result.Error);
    }

    [Fact]
    public async Task Add_Valid_AddsToDb()
    {
        ChampionshipAddRequest request = new("DailyRugby", Seasons.Season1);
        var result = await _champService.AddAsync(request);
        var all = await _champService.GetAllAsync();

        Assert.True(result.IsSuccessful);
        Assert.Contains(all, temp => temp.Id == result.Item.Id);
    }

    #endregion

    #region GetAll

    [Fact]
    public async Task GetAll_NoChampionships_ReturnsEmptyList()
    {
        var list = await _champService.GetAllAsync();
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetAll_AddFewChampionships_ReturnsAll()
    {
        List<ChampionshipAddRequest> requests =
            [
                new("champ1", Seasons.Season1),
                new("champ2", Seasons.Season1),
                new("champ3", Seasons.Season1)
            ];
        foreach (var request in requests) await _champService.AddAsync(request);

        var list = await _champService.GetAllAsync();
        Assert.Equal(requests.Count, list.Count);
        foreach (var response in list)
        {
            Assert.Contains(requests, temp => temp.Name == response.Name);
        }
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_NoMatch_ReturnsNotFound()
    {
        var result = await _champService.GetByIdAsync(Guid.NewGuid());
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetById_Match_ReturnsMatch()
    {
        List<ChampionshipAddRequest> requests =
            [
                new("champ1", Seasons.Season1),
                new("champ2", Seasons.Season1),
                new("champ3", Seasons.Season1)
            ];

        List<Result<ChampionshipResponse>> addResponses = [];
        foreach (var request in requests)
        {
            addResponses.Add(await _champService.AddAsync(request));
        }

        Assert.NotEmpty(addResponses);
        foreach (var addResponse in addResponses)
        {
            Assert.True(addResponse.IsSuccessful);
            var getByIdResult = await _champService.GetByIdAsync(addResponse.Item.Id);
            Assert.True(getByIdResult.IsSuccessful);
            Assert.Equal(addResponse.Item.Id, getByIdResult.Item.Id);
        }
    }

    #endregion

    #region GetStandings

    [Fact]
    public async Task GetStandings_NoTeams_ReturnsEmptyList()
    {
        var champ = await SetUpChampionship();
        var standings = await _champService.GetStandingsAsync(champ.Id);
        Assert.Empty(standings);
    }

    [Fact]
    public async Task GetStandings_NoGamesPlayed_ReturnsAllZeros()
    {
        var champ = await SetUpChampionship();
        await SetUpFourTeams(champ.Id);
        await _gameService.GenerateRounds(champ.Id);
        var standings = await _champService.GetStandingsAsync(champ.Id);

        Assert.NotEmpty(standings);
        int i = 1;
        foreach (var pair in standings)
        {
            Assert.Equal(0, pair.Value.WinCount);
            Assert.Equal(0, pair.Value.TieCount);
            Assert.Equal(0, pair.Value.LossCount);
            Assert.Equal(0, pair.Value.ScoredTriesCount);
            Assert.Equal(i, pair.Key);
            i++;
        }
    }

    [Fact]
    public async Task GetStandings_FirstRoundPlayed_ReturnsCorrectResults()
    {
        var champ = await SetUpChampionship();
        await SetUpFourTeams(champ.Id);
        await _gameService.GenerateRounds(champ.Id);
        await _champService.SetAsMainAsync(champ.Id);
        var firstRoundResult = await _gameService.GetCurrentRoundAsync();
        Assert.True(firstRoundResult.IsSuccessful);

        var simulator = new SeasonOneGameSimulator();
        foreach (var gameResponse in firstRoundResult.Item)
        {
            var game = await _db.Games
                .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
                .ThenInclude(temp => temp.Team)
                .FirstAsync(temp => temp.Id == gameResponse.Id);

            while (game.CurrentMinute <= 80)
            {
                var gameEvent = simulator.SimulateNextMinute(game);
                await simulator.SaveGameAsync(gameEvent, _db);
            }
        }

        var standings = await _champService.GetStandingsAsync(champ.Id);
        for (int i = 2; i <= standings.Count; i++)
        {
            var ahead = standings[i - 1];
            var behind = standings[i];

            Assert.True(ahead.WinCount >= behind.WinCount);
            if (ahead.WinCount != behind.WinCount) continue;

            Assert.True(ahead.PointsScored - ahead.PointsTaken
                >= behind.PointsScored - behind.PointsTaken);
            if (ahead.PointsScored - ahead.PointsTaken
                != behind.PointsScored - behind.PointsTaken) continue;

            Assert.True(ahead.ScoredTriesCount - ahead.SufferedTriesCount
                >= behind.ScoredTriesCount - behind.SufferedTriesCount);
        }
    }

    #endregion

    #region SetAsMain

    [Fact]
    public async Task SetAsMain_NotFoundId_ReturnsNotFound()
    {
        var result = await _champService.SetAsMainAsync(Guid.NewGuid());
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NotFound, result.Error);
    }

    [Fact]
    public async Task SetAsMain_SetsAsMainChampionship()
    {
        ChampionshipAddRequest request = new("champ", Seasons.Season1);
        var addResult = await _champService.AddAsync(request);
        Assert.True(addResult.IsSuccessful);

        var setAsMainResult = await _champService.SetAsMainAsync(addResult.Item.Id);
        var main = await _db.Championships.FirstOrDefaultAsync(temp => temp.IsMainChampionship);

        Assert.True(setAsMainResult.IsSuccessful);
        Assert.NotNull(main);
        Assert.Equal(main.Id, setAsMainResult.Item.Id);
    }

    [Fact]
    public async Task SetAsMain_ThereIsAlreadyMain_ReturnsInvalid()
    {
        ChampionshipAddRequest request1 = new("champ", Seasons.Season1);
        var addResult1 = await _champService.AddAsync(request1);
        Assert.True(addResult1.IsSuccessful);
        var mainResult1 = await _champService.SetAsMainAsync(addResult1.Item.Id);
        Assert.True(mainResult1.IsSuccessful);

        ChampionshipAddRequest request2 = new("champ2", Seasons.Season1);
        var addResult2 = await _champService.AddAsync(request2);
        Assert.True(addResult2.IsSuccessful);

        var mainResult2 = await _champService.SetAsMainAsync(addResult2.Item.Id);
        Assert.False(mainResult2.IsSuccessful);
        Assert.Equal(Errors.Invalid, mainResult2.Error);
    }

    #endregion

    #region UnsetAsMain

    [Fact]
    public async Task UnsetAsMain_NotFoundId_ReturnsNotFound()
    {
        var result = await _champService.UnsetAsMainAsync(Guid.NewGuid());
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NotFound, result.Error);
    }

    [Fact]
    public async Task UnsetAsMain_MakesIsMainFalse()
    {
        ChampionshipAddRequest request = new("champ", Seasons.Season1);
        var addResult = await _champService.AddAsync(request);
        Assert.True(addResult.IsSuccessful);
        var setAsMainResult = await _champService.SetAsMainAsync(addResult.Item.Id);
        Assert.True(setAsMainResult.IsSuccessful);

        var unsetAsMainResult = await _champService.UnsetAsMainAsync(addResult.Item.Id);
        Assert.True(unsetAsMainResult.IsSuccessful);
        Assert.Empty(await _db.Championships.Where(temp => temp.IsMainChampionship).ToListAsync());
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_NoMatchingId_ReturnsNotFound()
    {
        var result = await _champService.DeleteAsync(Guid.NewGuid());
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NotFound, result.Error);
    }

    [Fact]
    public async Task Delete_MatchingId_Deletes()
    {
        List<ChampionshipAddRequest> requests =
            [
                new("champ1", Seasons.Season1),
                new("champ2", Seasons.Season1),
                new("champ3", Seasons.Season1)
            ];

        List<Result<ChampionshipResponse>> addResponses = [];
        foreach (var request in requests)
        {
            addResponses.Add(await _champService.AddAsync(request));
        }

        Assert.NotEmpty(addResponses);
        foreach (var addResponse in addResponses)
        {
            Assert.True(addResponse.IsSuccessful);
            var deleteResult = await _champService.DeleteAsync(addResponse.Item.Id);
            Assert.True(deleteResult.IsSuccessful);
            Assert.DoesNotContain(addResponse.Item, await _champService.GetAllAsync());
        }
        Assert.Empty(await _champService.GetAllAsync());
    }

    #endregion

    #region Helpers

    private async Task<ChampionshipResponse> SetUpChampionship()
    {
        ChampionshipAddRequest request = new("Champ", Seasons.Season1);
        var result = await _champService.AddAsync(request);
        Assert.True(result.IsSuccessful);
        return result.Item;
    }

    private async Task<(TeamResponse TeamA,
        TeamResponse TeamB,
        TeamResponse TeamC,
        TeamResponse TeamD)> SetUpFourTeams(Guid champId)
    {
        return (await SetUpTeam(champId, 30, 35, 30),
            await SetUpTeam(champId, 40, 20, 35),
            await SetUpTeam(champId, 50, 20, 25),
            await SetUpTeam(champId, 25, 25, 45));
    }

    private async Task<TeamResponse> SetUpTeam(Guid champId,
        int insight,
        int physique,
        int technique)
    {
        TeamAddRequest request = new(champId,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            insight,
            physique,
            technique,
            Coaches.Insight);
        var result = await _teamService.AddAsync(request);
        Assert.True(result.IsSuccessful);
        return result.Item;
    }

    #endregion
}