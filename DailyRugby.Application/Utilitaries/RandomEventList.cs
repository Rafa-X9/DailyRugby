namespace DailyRugby.Application.Utilitaries;

public class RandomEventList<T>(Random random)
{
    private readonly List<Event> _events = [];
    private Func<T>? _fallback;
    private readonly Random _random = random;

    public RandomEventList<T> Add(double chance, Func<T> eventAction)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(chance);
        _events.Add(new Event(chance, eventAction));
        return this;
    }

    public RandomEventList<T> AddFallback(Func<T> fallback)
    {
        _fallback = fallback;
        return this;
    }

    public T Draw()
    {
        if (_events.Count == 0)
        {
            if (_fallback is not null)
                return _fallback();

            throw new InvalidOperationException("The random event list contains no events or fallback.");
        }

        double totalChance = _events.Sum(e => e.Chance);

        double normalization = totalChance > 1
            ? 1 / totalChance
            : 1;

        double roll = _random.NextDouble();

        double chanceSum = 0;

        foreach (var @event in _events)
        {
            chanceSum += @event.Chance * normalization;

            if (roll < chanceSum)
                return @event.Action();
        }

        if (_fallback is not null)
            return _fallback();

        throw new InvalidOperationException("No event was drawn and no fallback was configured.");
    }

    private sealed record Event(double Chance, Func<T> Action);
}