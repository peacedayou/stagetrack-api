namespace StageTrack.Api.Models;

public enum StatutCandidature
{
    AEnvoyer,
    Envoyee,
    Entrevue,
    Offre,
    Refusee
}

public class Candidature
{
    public int Id { get; set; }
    public required string Poste { get; set; }
    public StatutCandidature Statut { get; set; } = StatutCandidature.AEnvoyer;
    public DateOnly? DateEnvoi { get; set; }
    public string? Lien { get; set; }
    public string? Notes { get; set; }

    public int EntrepriseId { get; set; }
    public Entreprise? Entreprise { get; set; }
}
