using Microsoft.EntityFrameworkCore;
using StageTrack.Api.Models;

namespace StageTrack.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Entreprise> Entreprises => Set<Entreprise>();
    public DbSet<Candidature> Candidatures => Set<Candidature>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entreprise>(e =>
        {
            e.Property(x => x.Nom).HasMaxLength(150);
            e.HasIndex(x => x.Nom).IsUnique();
        });

        modelBuilder.Entity<Candidature>(c =>
        {
            c.Property(x => x.Poste).HasMaxLength(200);
            // Le statut est enregistré en texte pour rester lisible dans la base
            c.Property(x => x.Statut).HasConversion<string>().HasMaxLength(20);
            c.HasOne(x => x.Entreprise)
             .WithMany(x => x.Candidatures)
             .HasForeignKey(x => x.EntrepriseId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
