namespace DailyRugby.Domain;

public sealed class Championship
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public ChampionshipState State = ChampionshipState.NotStarted;
    public Seasons Season = Seasons.Season1;
    public IList<Team> Teams { get; set; } = [];
    public IList<Game> Games { get; set; } = [];
}