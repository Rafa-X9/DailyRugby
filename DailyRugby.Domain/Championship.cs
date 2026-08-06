namespace DailyRugby.Domain;

public sealed class Championship
{
    public Guid Id { get; }
    public string Name { get; } = string.Empty;
    public RuleSet Rules { get; }
    public IList<Team> Teams { get; } = [];
    public IList<Game> Games { get; set; } = [];
}