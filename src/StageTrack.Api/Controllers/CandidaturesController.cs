using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageTrack.Api.Data;
using StageTrack.Api.Dtos;
using StageTrack.Api.Models;

namespace StageTrack.Api.Controllers;

[ApiController]
[Route("api/candidatures")]
public class CandidaturesController(AppDbContext db) : ControllerBase
{
    private IQueryable<CandidatureDto> Projection(IQueryable<Candidature> source) =>
        source.Select(c => new CandidatureDto(
            c.Id, c.Poste, c.Statut, c.DateEnvoi, c.Lien, c.Notes, c.EntrepriseId, c.Entreprise!.Nom));

    /// <summary>Liste les candidatures, filtrables par statut ou par texte (poste ou entreprise).</summary>
    [HttpGet]
    public async Task<IEnumerable<CandidatureDto>> GetAll(
        [FromQuery] StatutCandidature? statut, [FromQuery] string? q)
    {
        var query = db.Candidatures.AsQueryable();
        if (statut is not null)
            query = query.Where(c => c.Statut == statut);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Poste.Contains(q) || c.Entreprise!.Nom.Contains(q));

        return await Projection(query.OrderByDescending(c => c.Id)).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CandidatureDto>> GetById(int id)
    {
        var dto = await Projection(db.Candidatures.Where(c => c.Id == id)).FirstOrDefaultAsync();
        return dto is null ? NotFound() : dto;
    }

    [HttpPost]
    public async Task<ActionResult<CandidatureDto>> Create(CandidatureCreateDto input)
    {
        if (!await db.Entreprises.AnyAsync(e => e.Id == input.EntrepriseId))
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(input.EntrepriseId)] = ["Entreprise introuvable."]
            }));

        var candidature = new Candidature
        {
            Poste = input.Poste.Trim(),
            EntrepriseId = input.EntrepriseId,
            Statut = input.Statut,
            DateEnvoi = input.DateEnvoi,
            Lien = input.Lien,
            Notes = input.Notes
        };
        db.Candidatures.Add(candidature);
        await db.SaveChangesAsync();

        var dto = await Projection(db.Candidatures.Where(c => c.Id == candidature.Id)).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = candidature.Id }, dto);
    }

    /// <summary>Change uniquement le statut. Passer à « Envoyee » fixe la date d'envoi si elle est vide.</summary>
    [HttpPatch("{id:int}/statut")]
    public async Task<ActionResult<CandidatureDto>> UpdateStatut(int id, StatutUpdateDto input)
    {
        var candidature = await db.Candidatures.FindAsync(id);
        if (candidature is null) return NotFound();

        candidature.Statut = input.Statut!.Value;
        if (candidature.Statut == StatutCandidature.Envoyee && candidature.DateEnvoi is null)
            candidature.DateEnvoi = DateOnly.FromDateTime(DateTime.Today);

        await db.SaveChangesAsync();
        return await Projection(db.Candidatures.Where(c => c.Id == id)).FirstAsync();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await db.Candidatures.Where(c => c.Id == id).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound() : NoContent();
    }
}
