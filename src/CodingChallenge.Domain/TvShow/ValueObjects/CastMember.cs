namespace CodingChallenge.Domain.TvShow.ValueObjects;

public record CastMember
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime? BirthDate { get; set; }
}
