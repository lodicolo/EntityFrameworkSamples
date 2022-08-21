
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leaderboard.Data;

public class Person
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = default!;

    public ICollection<LeaderboardEntry> LeaderboardEntries { get; private set; } = new List<LeaderboardEntry>();
}
