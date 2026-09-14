using DailyRugby.Application.Utilitaries;

namespace DailyRugby.Tests;
public class RandomEventListTests
{
    [Fact]
    public void EventList_NoRollWithFallBack_RunsFallBack()
    {
        const string fallback = "This is the fallback";

        RandomEventList<string> eventList = new(new Random());

        eventList
            .Add(0.5, () => "This has a 50% chance")
            .Add(0.2, () => "This has a 20% chance")
            .Add(0.1, () => "This has a 10% chance")
            .AddFallback(() => "This is the fallback");

        eventList
            .Add(0.0, () => "Impossible 1")
            .Add(0.0, () => "Impossible 2")
            .Add(0.0, () => "Impossible 3")
            .AddFallback(() => fallback);

        string result = eventList.Draw();

        Assert.Equal(fallback, result);
    }
}
