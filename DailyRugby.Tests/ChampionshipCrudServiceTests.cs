using DailyRugby.Application.CRUD;
using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Shared;

namespace DailyRugby.Tests;

public class ChampionshipCrudServiceTests
{
    private readonly IChampionshipCrudService _champService;

    public ChampionshipCrudServiceTests()
    {
        _champService = new ChampionshipCrudService();
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
        ChampionshipAddRequest request = new("   ");
        var result = await _champService.AddAsync(request);
        Assert.False(result.IsSuccessful);
        Assert.Equal(Errors.Invalid, result.Error);
    }

    [Fact]
    public async Task Add_Valid_AddsToDb()
    {
        ChampionshipAddRequest request = new("DailyRugby");
        var result = await _champService.AddAsync(request);
        var all = await _champService.GetAllAsync();

        Assert.True(result.IsSuccessful);
        Assert.Contains(result.Item, all);
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
                new("champ1"),
                new("champ2"),
                new("champ3")
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
                new("champ1"),
                new("champ2"),
                new("champ3")
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
            Assert.Equal(addResponse.Item, getByIdResult.Item);
        }
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
                new("champ1"),
                new("champ2"),
                new("champ3")
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
}