namespace DailyRugby.Domain;

public sealed class Championship
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ChampionshipState State = ChampionshipState.NotStarted;
    public IList<Team> Teams { get; set; } = [];
    public IList<Game> Games { get; set; } = [];
}