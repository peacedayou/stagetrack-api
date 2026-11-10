namespace StageTrack.Api.Models;

public class Entreprise
{
    public int Id { get; set; }
    public required string Nom { get; set; }
    public string? Ville { get; set; }
    public string? SiteWeb { get; set; }

    public List<Candidature> Candidatures { get; set; } = [];
}
