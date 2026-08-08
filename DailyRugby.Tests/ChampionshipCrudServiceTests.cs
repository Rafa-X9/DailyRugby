using DailyRugby.Application.CRUD;
using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
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

            ];
    }

    #endregion

}