using System.ComponentModel.DataAnnotations;

namespace DailyRugby.Domain;

public class Cake
{
    public Guid Id { get; set; }

    public Guid TeamId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
