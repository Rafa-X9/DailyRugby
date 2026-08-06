namespace DailyRugby.Domain;

public sealed class Championship(string name, RuleSet rules)
{
    public Guid Id { get; }
    public string Name { get; } = name;
    public RuleSet Rules { get; } = rules;
    public IList<Team> Teams { get; } = [];
    public IList<Game> Games { get; set; } = [];
}