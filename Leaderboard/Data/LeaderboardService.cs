using Leaderboard.EntityFrameworkExtensions;

using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Data;

public class LeaderboardService
{
    private readonly LeaderboardContext _context;

    public LeaderboardService(LeaderboardContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<LeaderboardEntry>> GetEntriesAsync(
        Guid? id,
        bool? orderAscending,
        int? page,
        Guid? filterPersonId,
        Guid? findPersonId,
        int? teamSize
    )
    {
        IQueryable<LeaderboardEntry> query = _context.LeaderboardEntries;

        int? skip = null;
        int? take = null;
        if (id == null)
        {
            if (findPersonId != null)
            {
                var filterId = (Guid)findPersonId;
                IQueryable<LeaderboardEntry> filterQuery = _context.LeaderboardEntries
                    .Where(entry => entry.TeamSize == (teamSize ?? 4))
                    .Include(entry => entry.Members);

                if (orderAscending.HasValue)
                {
                    if (orderAscending.Value)
                    {
                        filterQuery = filterQuery.OrderBy(entry => entry.Score);
                    }
                    else
                    {
                        filterQuery = filterQuery.OrderByDescending(entry => entry.Score);
                    }
                }
                var entryRow = filterQuery
                    // This also blows up
                    //.TakeWhile(entry => entry.Members.Any(member => member.Id != filterId))

                    // The translator plugin isn't working so it blows up on OrderBy
                    //.Select(entry => EF.Functions.RowNumber(EF.Functions.OrderBy(entry.Score)))
                    //.SkipWhile(entry => entry.Entry.Members.All(member => member.Id != filterPersonId))
                    //.Take(1)
                    .FirstOrDefault();
                entryRow.ToString();
            }
            else
            {
                if (filterPersonId == null)
                {
                    query = query.Where(entry => entry.TeamSize == (teamSize ?? 4));
                }
                else
                {
                    if (teamSize != null)
                    {
                        query = query.Where(entry => entry.TeamSize == teamSize);
                    }

                    query = query
                        .Include(entry => entry.Members)
                        .Where(entry => entry.Members.Any(member => member.Id == filterPersonId));
                }

                if (orderAscending.HasValue)
                {
                    if (orderAscending.Value)
                    {
                        query = query.OrderBy(entry => entry.Score);
                    }
                    else
                    {
                        query = query.OrderByDescending(entry => entry.Score);
                    }
                }

                skip = 10 * (page ?? 0);
                take = 10;
            }
        }
        else
        {
            query = query
                .Where(entry => entry.Id == id);

            take = 1;
        }

        query = query
            .Include(entry => entry.Members);

        var count = await query.CountAsync();

        var valueQuery = query;
        if (skip != null)
        {
            valueQuery = valueQuery.Skip((int)skip);
        }

        if (take != null)
        {
            valueQuery = valueQuery.Take((int)take);
        }

        var values = await valueQuery.ToArrayAsync();
        var result = new PagedResult<LeaderboardEntry>
        {
            Count = count,
            Page = Math.Min(page ?? 0, count / 10),
            Values = values.ToArray()
        };

        return result;
    }

    public async Task<PagedResult<Person>> GetPeopleAsync(
        Guid? id,
        bool includeEntries,
        bool? orderAscending,
        int? page
    )
    {
        IQueryable<Person> query = _context.People;

        int? skip = null;
        int? take = null;
        if (id == null)
        {
            if (orderAscending.HasValue)
            {
                if (orderAscending.Value)
                {
                    query = query.OrderBy(person => person.Name);
                }
                else
                {
                    query = query.OrderByDescending(person => person.Name);
                }
            }

            skip = 10 * (page ?? 0);
            take = 10;
        }
        else
        {
            query = query
                .Where(person => person.Id == id);

            take = 1;
        }

        if (includeEntries)
        {
            query = query
                .Include(people => people.LeaderboardEntries);
        }

        var count = await query.CountAsync();

        var valueQuery = query;
        if (skip != null)
        {
            valueQuery = valueQuery.Skip((int)skip);
        }

        if (take != null)
        {
            valueQuery = valueQuery.Take((int)take);
        }

        var values = await valueQuery.ToArrayAsync();
        var result = new PagedResult<Person>
        {
            Count = count,
            Page = Math.Min(page ?? 0, count / 10),
            Values = values
        };

        return result;
    }

    public async Task AddPersonAsync(Person person)
    {
        _ = await _context.People.AddAsync(person);
        _ = await _context.SaveChangesAsync();
    }
}
