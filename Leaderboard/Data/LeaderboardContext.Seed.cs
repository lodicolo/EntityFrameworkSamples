using Microsoft.EntityFrameworkCore;

using System.Globalization;
using System.Security.Cryptography;

namespace Leaderboard.Data;

public partial class LeaderboardContext
{
    public void Seed(ModelBuilder modelBuilder)
    {
        var randomPeople = new List<Person>();
        while (randomPeople.Count < 50000)
        {
            randomPeople.Add(new Person
            {
                Name = GenerateName(),
            });
        }

        using var rng = RandomNumberGenerator.Create();
        var randomLeaderboardEntries = new List<LeaderboardEntry>();
        var randomLeaderboardEntryMembers = new List<(Guid Entry, Guid Member)>();
        var rngBuffer = new byte[4];
        while (randomLeaderboardEntries.Count < randomPeople.Count)
        {
            rng.GetBytes(rngBuffer);
            var score = BitConverter.ToUInt32(rngBuffer) % int.MaxValue;
            var teamSize = 1 + BitConverter.ToUInt32(rngBuffer) % 4;
            var entry = new LeaderboardEntry
            {
                Score = (int)score,
                TeamSize = (int)teamSize,
            };
            randomLeaderboardEntries.Add(entry);

            rng.GetBytes(rngBuffer);
            var members = new List<(Guid Entry, Guid Member)>();
            while (teamSize-- > 0)
            {
                rng.GetBytes(rngBuffer);
                var personNumber = BitConverter.ToUInt32(rngBuffer) % randomPeople.Count;
                var person = randomPeople[(int)personNumber];
                while (members.Any(junction => junction.Member == person.Id))
                {
                    rng.GetBytes(rngBuffer);
                    personNumber = BitConverter.ToUInt32(rngBuffer) % randomPeople.Count;
                    person = randomPeople[(int)personNumber];
                }
                var member = (Entry: entry.Id, Member: person.Id);
                members.Add(member);
            }
            randomLeaderboardEntryMembers.AddRange(members);
        }

        _ = modelBuilder.Entity<Person>()
            .HasData(randomPeople);

        _ = modelBuilder.Entity<LeaderboardEntry>()
            .HasData(randomLeaderboardEntries);

        _ = modelBuilder.Entity<LeaderboardEntry>()
            .HasMany(entry => entry.Members)
            .WithMany(person => person.LeaderboardEntries)
            .UsingEntity<Dictionary<object, object>>(
                "LeaderboardEntityPerson",
                r => r.HasOne<Person>().WithMany().HasForeignKey("MembersId"),
                l => l.HasOne<LeaderboardEntry>().WithMany().HasForeignKey("LeaderboardEntriesId"),
                je =>
                {
                    je.HasKey("LeaderboardEntriesId", "MembersId");
                    je.HasData(
                        randomLeaderboardEntryMembers
                            .Select(junction => new
                            {
                                LeaderboardEntriesId = junction.Entry,
                                MembersId = junction.Member
                            })
                            .ToArray()
                    );
                }
            );

        //_ = modelBuilder.Entity<LeaderboardEntryMember>()
        //    .HasData(randomLeaderboardEntryMembers);
    }

    public static string GenerateName()
    {
        using var rng = RandomNumberGenerator.Create();

        var intBuffer = new byte[4];

        rng.GetBytes(intBuffer);
        var adjective1Index = BitConverter.ToUInt32(intBuffer) % Adjectives.Length;

        var adjective2Index = adjective1Index;
        while (adjective2Index == adjective1Index)
        {
            rng.GetBytes(intBuffer);
            adjective2Index = BitConverter.ToUInt32(intBuffer) % Adjectives.Length;
        }

        rng.GetBytes(intBuffer);
        var animalIndex = BitConverter.ToUInt32(intBuffer) % Animals.Length;

        return $"{Adjectives[adjective1Index]}{Adjectives[adjective2Index]}{Animals[animalIndex]}";
    }

    private readonly static string[] Adjectives = File
        .ReadAllText("adjectives.txt")
        .Split('\n')
        .Select(CultureInfo.CurrentCulture.TextInfo.ToTitleCase)
        .ToArray();

    private readonly static string[] Animals = File
        .ReadAllText("animals.txt")
        .Split('\n')
        .Select(CultureInfo.CurrentCulture.TextInfo.ToTitleCase)
        .ToArray();
}
