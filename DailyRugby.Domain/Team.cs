namespace DailyRugby.Domain;

public class Team
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid ChampionshipId { get; set; }
    public string PlayerUsername { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Insight { get; set; }
    public int Physique { get; set; }
    public int Technique { get; set; }
    public bool HasInsigthCoach { get; set; }
    public bool HasPhysiqueCoach { get; set; }
    public bool HasTechniqueCoach { get; set; }
    public bool HasGeneralCoach { get; set; }
    public int CakesAmount { get; set; }
}