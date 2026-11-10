using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageTrack.Api.Data;
using StageTrack.Api.Dtos;
using StageTrack.Api.Models;

namespace StageTrack.Api.Controllers;

[ApiController]
[Route("api/entreprises")]
public class EntreprisesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<EntrepriseDto>> GetAll() =>
        await db.Entreprises
            .OrderBy(e => e.Nom)
            .Select(e => new EntrepriseDto(e.Id, e.Nom, e.Ville, e.SiteWeb, e.Candidatures.Count))
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EntrepriseDto>> GetById(int id)
    {
        var dto = await db.Entreprises
            .Where(e => e.Id == id)
            .Select(e => new EntrepriseDto(e.Id, e.Nom, e.Ville, e.SiteWeb, e.Candidatures.Count))
            .FirstOrDefaultAsync();
        return dto is null ? NotFound() : dto;
    }

    [HttpPost]
    public async Task<ActionResult<EntrepriseDto>> Create(EntrepriseCreateDto input)
    {
        var nom = input.Nom.Trim();
        if (await db.Entreprises.AnyAsync(e => e.Nom == nom))
            return Conflict(new ProblemDetails { Title = $"L'entreprise « {nom} » existe déjà." });

        var entreprise = new Entreprise { Nom = nom, Ville = input.Ville, SiteWeb = input.SiteWeb };
        db.Entreprises.Add(entreprise);
        await db.SaveChangesAsync();

        var dto = new EntrepriseDto(entreprise.Id, entreprise.Nom, entreprise.Ville, entreprise.SiteWeb, 0);
        return CreatedAtAction(nameof(GetById), new { id = entreprise.Id }, dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await db.Entreprises.Where(e => e.Id == id).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound() : NoContent();
    }
}
