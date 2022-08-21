
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leaderboard.Data;

public class LeaderboardEntry
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();

    public int Score { get; set; }

    public int TeamSize { get; set; }

    public ICollection<Person> Members { get; private set; } = new List<Person>();
}
