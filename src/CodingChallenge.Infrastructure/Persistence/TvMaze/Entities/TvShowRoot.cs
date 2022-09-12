namespace CodingChallenge.Infrastructure.Persistence.TvMaze.Entities;

/// <summary>
/// generated from TVMaze.com show json response
/// </summary>

public class Cast
{
    public Person? person { get; set; }
    public Character? character { get; set; }
    public bool self { get; set; }
    public bool voice { get; set; }
}

public class Character
{
    public int? id { get; set; }
    public string url { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public Image? image { get; set; }
    public Links? _links { get; set; }
}

public class Country
{
    public string name { get; set; } = string.Empty;
    public string code { get; set; } = string.Empty;
    public string timezone { get; set; } = string.Empty;
}

public class Embedded
{
    public List<Cast>? cast { get; set; }
}

public class Externals
{
    public int? tvrage { get; set; }
    public int? thetvdb { get; set; }
    public string imdb { get; set; } = string.Empty;
}

public class Image
{
    public string medium { get; set; } = string.Empty;
    public string original { get; set; } = string.Empty;
}

public class Links
{
    public Self? self { get; set; }
    public Previousepisode? previousepisode { get; set; }
}

public class Network
{
    public int? id { get; set; }
    public string name { get; set; } = string.Empty;
    public Country? country { get; set; }
    public string officialSite { get; set; } = string.Empty;
}

public class Person
{
    public int? id { get; set; }
    public string url { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public Country? country { get; set; }
    public string birthday { get; set; } = string.Empty;
    public string deathday { get; set; } = string.Empty;
    public string gender { get; set; } = string.Empty;
    public Image? image { get; set; }
    public int? updated { get; set; }
    public Links? _links { get; set; }
}

public class Previousepisode
{
    public string href { get; set; } = string.Empty;
}

public class Rating
{
    public double? average { get; set; }
}

public class TvShowRoot
{
    public int? id { get; set; }
    public string url { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string language { get; set; } = string.Empty;
    public List<string>? genres { get; set; }
    public string status { get; set; } = string.Empty;
    public int? runtime { get; set; }
    public int? averageRuntime { get; set; }
    public string premiered { get; set; } = string.Empty;
    public string ended { get; set; } = string.Empty;
    public string officialSite { get; set; } = string.Empty;
    public Schedule? schedule { get; set; }
    public Rating? rating { get; set; }
    public int? weight { get; set; }
    public Network? network { get; set; }
    public Externals? externals { get; set; }
    public Image? image { get; set; }
    public string summary { get; set; } = string.Empty;
    public int? updated { get; set; }
    public Links? _links { get; set; }
    public Embedded? _embedded { get; set; }
}

public class Schedule
{
    public string time { get; set; } = string.Empty;
    public List<string>? days { get; set; }
}

public class Self
{
    public string href { get; set; } = string.Empty;
}

