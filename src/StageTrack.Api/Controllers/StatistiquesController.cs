using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageTrack.Api.Data;
using StageTrack.Api.Dtos;
using StageTrack.Api.Models;

namespace StageTrack.Api.Controllers;

[ApiController]
[Route("api/statistiques")]
public class StatistiquesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<StatistiquesDto> Get()
    {
        var parStatut = await db.Candidatures
            .GroupBy(c => c.Statut)
            .Select(g => new { Statut = g.Key, Nombre = g.Count() })
            .ToListAsync();

        var total = parStatut.Sum(x => x.Nombre);
        var dictionnaire = Enum.GetValues<StatutCandidature>()
            .ToDictionary(s => s.ToString(), s => parStatut.FirstOrDefault(x => x.Statut == s)?.Nombre ?? 0);

        // Taux d'entrevue : candidatures arrivées en entrevue ou plus loin, parmi celles envoyées
        var envoyees = total - dictionnaire[nameof(StatutCandidature.AEnvoyer)];
        var entrevues = dictionnaire[nameof(StatutCandidature.Entrevue)] + dictionnaire[nameof(StatutCandidature.Offre)];
        var taux = envoyees == 0 ? 0 : Math.Round(100.0 * entrevues / envoyees, 1);

        return new StatistiquesDto(total, dictionnaire, taux);
    }
}
