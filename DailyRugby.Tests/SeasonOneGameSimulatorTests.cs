using DailyRugby.Application.Simulators;
using DailyRugby.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static DailyRugby.Application.Simulators.SeasonOneGameSimulator;

namespace DailyRugby.Tests;

public class SeasonOneGameSimulatorTests
{
    //Stats stats1 = new(30, 35, 30);
    //Stats stats2 = new(50, 20, 25);

    #region Counts

    [Fact]
    public void TryCount_IsCorrect()
    {
        // ((physique * .7) + (insight * .2) + (technique * .1)) / 3, rounds down

        Stats stats1 = new(30, 35, 30);
        int count1 = stats1.GetTryCount();
        Assert.Equal(11, count1);

        Stats stats2 = new(50, 20, 25);
        int count2 = stats2.GetTryCount();
        Assert.Equal(8, count2);
    }

    [Fact]
    public void DropGoalCount_IsCorrect()
    {
        // ((insight * .8) + (technique * .2)) / 10, rounds down

        Stats stats1 = new(30, 35, 30);
        int count1 = stats1.GetDropGoalCount();
        Assert.Equal(3, count1);

        Stats stats2 = new(50, 20, 25);
        int count2 = stats2.GetDropGoalCount();
        Assert.Equal(4, count2);
    }

    [Fact]
    public void PenaltyCount_IsCorrect()
    {
        // ( 50 - ((insight * .8) + (technique * .2)) ) / 4, rounds down

        Stats stats1 = new(30, 35, 30);
        int count1 = stats1.GetOpponentPenaltyCount();
        Assert.Equal(5, count1);

        Stats stats2 = new(50, 20, 25);
        int count2 = stats2.GetOpponentPenaltyCount();
        Assert.Equal(1, count2);
    }

    #endregion

    #region Chances

    [Fact]
    public void TrySuccessChance_IsCorrect()
    {
        // (insight * .7 + technique * .3)%

        Stats stats1 = new(30, 35, 30);
        double chance1 = stats1.GetTrySuccessChance();
        Assert.Equal(0.30, chance1, 0.01);

        Stats stats2 = new(50, 20, 25);
        double chance2 = stats2.GetTrySuccessChance();
        Assert.Equal(0.42, chance2, 0.01);
    }

    [Fact]
    public void ConversionSuccessChance_IsCorrect()
    {
        // (technique * .8 + insight * .2)%

        Stats stats1 = new(30, 35, 30);
        double chance1 = stats1.GetConversionSuccessChance();
        Assert.Equal(0.3, chance1, 0.1);

        Stats stats2 = new(50, 20, 25);
        double chance2 = stats2.GetConversionSuccessChance();
        Assert.Equal(0.3, chance2, 0.1);
    }

    [Fact]
    public void DropGoalSuccessChance_EqualsConversionSuccesChance()
    {
        Stats stats = new(30, 35, 30);
        Assert.Equal(stats.GetConversionSuccessChance(), stats.GetDropGoalSuccessChance());
    }

    [Fact]
    public void PenaltySuccessChance_EqualsConversionSuccesChance()
    {
        Stats stats = new(30, 35, 30);
        Assert.Equal(stats.GetConversionSuccessChance(), stats.GetPenaltySuccessChance());
    }

    #endregion
}