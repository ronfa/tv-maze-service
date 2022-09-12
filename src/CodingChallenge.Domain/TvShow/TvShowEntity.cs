using CodingChallenge.Domain.Base;
using CodingChallenge.Domain.TvShow.ValueObjects;

namespace CodingChallenge.Domain.TvShow;

public class TvShowEntity : AuditableEntity
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public IEnumerable<CastMember> Cast { get; set; } = Enumerable.Empty<CastMember>();
}