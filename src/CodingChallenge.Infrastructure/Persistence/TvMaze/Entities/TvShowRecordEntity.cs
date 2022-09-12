using CodingChallenge.Domain.Base;

namespace CodingChallenge.Infrastructure.Persistence.TvMaze.Entities;

public class TvShowRecordEntity : AuditableEntity
{
    public int Index { get; set; }

    public TvShowRoot? Show { get; set; } 
}
