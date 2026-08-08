using DailyRugby.Application.CRUD;
using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Validators;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Tests;

public class TeamCrudServiceTests : IAsyncLifetime
{
    private IChampionshipCrudService _champService = null!;
    private ITeamCrudService _teamService = null!;
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
        var result = await _teamService.AddAsync(null);
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NullArgument, result.Error);
    }

    [Fact]
    public async Task Add_EmptyPlayerUsername_ReturnsInvalid()
    {
        ChampionshipResponse champ = await SetUpChampionship();
        TeamAddRequest request = new(champ.Id,
            "   ",
            95,
            0,
            0,
            Coaches.Insight);

        var result = await _teamService.AddAsync(request);
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.Invalid, result.Error);
    }

    [Fact]
    public async Task Add_NoMatchingChampionship_ReturnsNotFound()
    {
        await SetUpChampionship();
        TeamAddRequest request = new(Guid.NewGuid(),
            "RafaX9",
            95, 0, 0,
            Coaches.Insight);

        var result = await _teamService.AddAsync(request);
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NotFound, result.Error);
    }

    [Fact]
    public async Task Add_StatLessThanZero_ReturnsInvalid()
    {
        var champ = await SetUpChampionship();

        List<TeamAddRequest> requests =
            [
            new(champ.Id,
                "agweg",
                96, -1, 0,
                Coaches.Insight),
            new(champ.Id,
                "ugikn",
                0, 96, -1,
                Coaches.Physique),
            new(champ.Id,
                "iugiug",
                0, -1, 96,
                Coaches.Technique)
            ];

        foreach (var request in requests)
        {
            var result = await _teamService.AddAsync(request);
            Assert.False(result.IsSuccessful);
            Assert.Equal(Errors.Invalid, result.Error);
        }
    }

    [Fact]
    public async Task Add_SeasonOne_DontSumUpTo95_ReturnsInvalid()
    {
        var champ = await SetUpChampionship(Seasons.Season1);
        List<TeamAddRequest> requests = 
            [
            new(champ.Id,
                "RafaX9",
                92, 1, 1,
                Coaches.Technique),
            new(champ.Id,
                "eqgf",
                1, 1, 94,
                Coaches.Insight)
            ];

        foreach (var request in requests)
        {
            var result = await _teamService.AddAsync(request);
            Assert.False(result.IsSuccessful);
            Assert.Equal(Errors.Invalid, result.Error);
        }
    }

    [Fact]
    public async Task Add_Valid_AddsToDb()
    {
        var champ = await SetUpChampionship();
        TeamAddRequest request = new(champ.Id,
            "RafaX9",
            93, 1, 1,
            Coaches.Technique);

        var result = await _teamService.AddAsync(request);
        Assert.True(result.IsSuccessful);
        Assert.Contains(await _teamService.GetAllAsync(champ.Id),
            temp => temp.Id == result.Item.Id);
    }

    #endregion

    #region GetAll

    [Fact]
    public async Task GetAll_NoTeams_ReturnsEmptyList()
    {
        var champ = await SetUpChampionship();
        var list = await _teamService.GetAllAsync(champ.Id);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetAll_AddFewTeams_ReturnsAll()
    {
        var champ = await SetUpChampionship();
        List<TeamResponse> addedTeams = [];
        for (int i = 0; i < 3; i++)
        {
            addedTeams.Add(await SetUpTeam(champ.Id));
        }

        var all = await _teamService.GetAllAsync(champ.Id);

        Assert.Equal(3, all.Count);
        foreach (var added in addedTeams)
        {
            Assert.Contains(all, temp => temp.Id == added.Id);
        }
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_NoMatch_ReturnsNotFound()
    {
        var result = await _teamService.GetByIdAsync(Guid.NewGuid());
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetById_Match_ReturnsMatch()
    {
        var champ = await SetUpChampionship();
        List<TeamResponse> addedTeams = [];
        for (int i = 0; i < 3; i++)
        {
            addedTeams.Add(await SetUpTeam(champ.Id));
        }

        foreach (var added in addedTeams)
        {
            var result = await _teamService.GetByIdAsync(added.Id);
            Assert.True(result.IsSuccessful);
            Assert.Equal(added.Id, result.Item.Id);
        }
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_NoMatch_ReturnsNotFound()
    {
        var champ = await SetUpChampionship();
        await SetUpChampionship();
        var result = await _teamService.DeleteAsync(Guid.NewGuid());
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.NotFound, result.Error);
    }

    [Fact]
    public async Task Delete_Match_Deletes()
    {
        var champ = await SetUpChampionship();
        List<TeamResponse> addedTeams = [];
        for (int i = 0; i < 3; i++)
        {
            addedTeams.Add(await SetUpTeam(champ.Id));
        }

        foreach (var added in addedTeams)
        {
            var result = await _teamService.DeleteAsync(added.Id);
            Assert.True(result.IsSuccessful);
            Assert.DoesNotContain(await _teamService.GetAllAsync(champ.Id),
                temp => temp.Id == added.Id);
        }
        Assert.Empty(await _teamService.GetAllAsync(champ.Id));
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

    private async Task<TeamResponse> SetUpTeam(Guid champId, int statSum = 95)
    {
        TeamAddRequest request = new(champId,
            "AFafgwe",
            statSum - 2, 1, 1,
            Coaches.Technique);
        var result = await _teamService.AddAsync(request);
        if (!result.IsSuccessful) throw new Exception();
        return result.Item;
    }

    #endregion
}