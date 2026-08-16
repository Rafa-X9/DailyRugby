using DailyRugby.Application.CRUD;
using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Validators;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Tests;

public class GameCrudServiceTests : IAsyncLifetime
{
    private IChampionshipCrudService _champService = null!;
    private ITeamCrudService _teamService = null!;
    private IGameCrudService _gameService = null!;
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;

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

        await _db.Database.EnsureDeletedAsync();
        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.CloseAsync();
    }

    #region GenerateRounds

    [Fact]
    public async Task GenerateRounds_IdNotFound_ReturnsNotFound()
    {
        await SetUpChampionship();
        var result = await _gameService.GenerateRounds(Guid.NewGuid());
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NotFound, result.Error);
    }

    [Fact]
    public async Task GenerateRounds_LessThanTwoTeams_ReturnsInvalid()
    {
        var champ = await SetUpChampionship();
        await SetUpTeamA(champ.Id, 95);
        var result = await _gameService.GenerateRounds(champ.Id);
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.Invalid, result.Error);
    }

    [Fact]
    public async Task GenerateRounds_ThreeTeams_GeneratesCorrectRoundAmount()
    {
        var champ = await SetUpChampionship();
        var teams = await SetUpThreeTeams(champ.Id, 95);

        var result = await _gameService.GenerateRounds(champ.Id);
        Assert.True(result.IsSuccessful);
        Assert.Equal(3, result.Item.Count);
        for (int i = 1; i <= 3; i++)
        {
            Assert.False(result.Item.Count(temp => temp.Round == i) > 1);
        }
    }

    [Fact]
    public async Task GenerateRounds_ThreeTeams_GeneratesCorrectPairings()
    {
        var champ = await SetUpChampionship();
        var teams = await SetUpThreeTeams(champ.Id, 95);

        var result = await _gameService.GenerateRounds(champ.Id);
        Assert.True(result.IsSuccessful);

        var teamAteamB = await GetGameBetween(teams.teamA.Id, teams.teamB.Id, result.Item);
        var teamAteamC = await GetGameBetween(teams.teamA.Id, teams.teamC.Id, result.Item);
        var teamBteamC = await GetGameBetween(teams.teamB.Id, teams.teamC.Id, result.Item);

        Assert.NotNull(teamAteamB);
        Assert.NotNull(teamAteamC);
        Assert.NotNull(teamBteamC);

        HashSet<int> rounds = [teamAteamB.Round, teamAteamC.Round, teamBteamC.Round];
        Assert.Equal(3, rounds.Count);
        Assert.Contains(1, rounds);
        Assert.Contains(2, rounds);
        Assert.Contains(3, rounds);
    }

    [Fact]
    public async Task GenerateRounds_FourTeams_GeneratesCorrectRoundAmount()
    {
        var champ = await SetUpChampionship();
        var teams = await SetUpFourTeams(champ.Id, 95);

        var result = await _gameService.GenerateRounds(champ.Id);
        Assert.True(result.IsSuccessful);
        Assert.Equal(6, result.Item.Count);
        for (int i = 1; i <= 3; i++)
        {
            Assert.False(result.Item.Count(temp => temp.Round == i) > 2);
        }
    }

    [Fact]
    public async Task GenerateRounds_FourTeams_GeneratesCorrectPairings()
    {
        var champ = await SetUpChampionship();
        var teams = await SetUpFourTeams(champ.Id, 95);

        var result = await _gameService.GenerateRounds(champ.Id);
        Assert.True(result.IsSuccessful);

        List<GameResponse?> games =
            [
                await GetGameBetween(teams.teamA.Id, teams.teamB.Id, result.Item),
                await GetGameBetween(teams.teamA.Id, teams.teamC.Id, result.Item),
                await GetGameBetween(teams.teamA.Id, teams.teamD.Id, result.Item),
                await GetGameBetween(teams.teamB.Id, teams.teamC.Id, result.Item),
                await GetGameBetween(teams.teamB.Id, teams.teamD.Id, result.Item),
                await GetGameBetween(teams.teamC.Id, teams.teamD.Id, result.Item),
            ];

        HashSet<int> rounds = [];

        foreach (var game in games)
        {
            Assert.NotNull(game);
            rounds.Add(game.Round);
        }
        Assert.Equal(3, rounds.Count);
        for (int i = 1; i <= 3; i++) Assert.Contains(i, rounds);
    }

    #endregion

    #region GetAll

    [Fact]
    public async Task GetAll_NoGames_ReturnsEmptyList()
    {
        var champ = await SetUpChampionship();
        var list = await _gameService.GetAllAsync(champ.Id);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetAll_HasGames_ReturnsGames()
    {
        var champ = await SetUpChampionship();
        var teams = await SetUpFourTeams(champ.Id, 95);
        var gamesResult = await _gameService.GenerateRounds(champ.Id);
        Assert.True(gamesResult.IsSuccessful);
        var games = gamesResult.Item;

        var list = await _gameService.GetAllAsync(champ.Id);
        Assert.Equal(games.Count, list.Count);
        foreach (var game in games)
        {
            Assert.Contains(list, temp => temp.Id == game.Id);
        }
    }

    #endregion

    #region GetByTeamId

    [Fact]
    public async Task GetByTeamId_NoMatch_ReturnsNotFound()
    {
        var champ = await SetUpChampionship();
        await SetUpTeamA(champ.Id, 95);
        var result = await _gameService.GetByTeamIdAsync(Guid.NewGuid());
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetByTeamId_Match_ReturnsAllGames()
    {
        var champ = await SetUpChampionship();
        var teams = await SetUpFourTeams(champ.Id, 95);
        List<TeamResponse> teamsList =
            [teams.teamA, teams.teamB, teams.teamC, teams.teamD];
        await _gameService.GenerateRounds(champ.Id);

        foreach (var team in teamsList)
        {
            var gamesResult = await _gameService.GetByTeamIdAsync(team.Id);
            Assert.True(gamesResult.IsSuccessful);
            Assert.Equal(3, gamesResult.Item.Count);
            Assert.DoesNotContain(gamesResult.Item, temp =>
                temp.TeamA.Team.Id != team.Id && temp.TeamB.Team.Id != team.Id);
        }
    }

    #endregion

    #region GetCurrentRound

    [Fact]
    public async Task GetCurrentRound_NoMainChampionship_ReturnsInvalid()
    {
        var champ = await SetUpChampionship();
        await SetUpFourTeams(champ.Id, 95);
        await _gameService.GenerateRounds(champ.Id);

        var result = await _gameService.GetCurrentRoundAsync();
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.Invalid, result.Error);
    }

    [Fact]
    public async Task GetCurrentRound_NoRounds_ReturnsInvalid()
    {
        var champ = await SetUpChampionship();
        await _champService.SetAsMainAsync(champ.Id);
        await SetUpFourTeams(champ.Id, 95);
        var result = await _gameService.GetCurrentRoundAsync();
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.Invalid, result.Error);
    }

    [Fact]
    public async Task GetCurrentRound_NoGamesFinished_ReturnsFirstRound()
    {
        var champ = await SetUpChampionship();
        await _champService.SetAsMainAsync(champ.Id);
        await SetUpFourTeams(champ.Id, 95);
        var gamesResult = await _gameService.GenerateRounds(champ.Id);
        Assert.True(gamesResult.IsSuccessful);
        var currentRoundResult = await _gameService.GetCurrentRoundAsync();

        Assert.True(currentRoundResult.IsSuccessful);
        Assert.Equal(gamesResult.Item.Count(temp => temp.Round == 1), currentRoundResult.Item.Count);

        foreach (var game in gamesResult.Item)
        {
            if (game.Round != 1) continue;
            Assert.Contains(currentRoundResult.Item, temp => temp.Id == game.Id);
        }
    }

    [Fact]
    public async Task GetCurrentRound_RoundOneFinished_ReturnsRoundTwo()
    {
        var champ = await SetUpChampionship();
        await _champService.SetAsMainAsync(champ.Id);
        await SetUpFourTeams(champ.Id, 95);
        var gamesResult = await _gameService.GenerateRounds(champ.Id);
        Assert.True(gamesResult.IsSuccessful);

        await _db.Games
            .Where(temp => temp.ChampionshipId == champ.Id
                && temp.Round == 1)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(temp => temp.CurrentState, GameState.Finished)
                .SetProperty(temp => temp.TeamAScore, 30)
                .SetProperty(temp => temp.TeamBScore, 20));

        var currentRoundResult = await _gameService.GetCurrentRoundAsync();
        Assert.True(currentRoundResult.IsSuccessful);
        Assert.Equal(gamesResult.Item.Count(temp => temp.Round == 2), currentRoundResult.Item.Count);

        foreach (var game in gamesResult.Item)
        {
            if (game.Round != 2) continue;
            Assert.Contains(currentRoundResult.Item, temp => temp.Id == game.Id);
        }
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