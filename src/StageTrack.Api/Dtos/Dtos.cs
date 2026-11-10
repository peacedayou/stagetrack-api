using System.ComponentModel.DataAnnotations;
using StageTrack.Api.Models;

namespace StageTrack.Api.Dtos;

public record EntrepriseDto(int Id, string Nom, string? Ville, string? SiteWeb, int NombreCandidatures);

public record EntrepriseCreateDto(
    [Required, StringLength(150, MinimumLength = 2)] string Nom,
    [StringLength(100)] string? Ville,
    [Url] string? SiteWeb);

public record CandidatureDto(
    int Id,
    string Poste,
    StatutCandidature Statut,
    DateOnly? DateEnvoi,
    string? Lien,
    string? Notes,
    int EntrepriseId,
    string Entreprise);

public record CandidatureCreateDto(
    [Required, StringLength(200, MinimumLength = 2)] string Poste,
    [Range(1, int.MaxValue)] int EntrepriseId,
    StatutCandidature Statut = StatutCandidature.AEnvoyer,
    DateOnly? DateEnvoi = null,
    [Url] string? Lien = null,
    [StringLength(2000)] string? Notes = null);

public record StatutUpdateDto([Required] StatutCandidature? Statut);

public record StatistiquesDto(int Total, Dictionary<string, int> ParStatut, double TauxEntrevue);
