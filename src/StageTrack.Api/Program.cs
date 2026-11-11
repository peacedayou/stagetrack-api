using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using StageTrack.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// La chaîne de connexion est lue au moment de créer le DbContext,
// ce qui permet aux tests de la remplacer facilement.
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlite(sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")
                      ?? "Data Source=stagetrack.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// CORS : origines autorisées lues dans la configuration (Cors:Origins),
// par exemple l'interface StageTrack Web servie depuis un autre domaine.
var origines = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origines).AllowAnyHeader().WithMethods("GET", "POST", "PATCH", "DELETE")));

var app = builder.Build();

// Crée la base au premier démarrage (projet de démonstration : pas de migrations)
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Rend la classe Program visible pour les tests d'intégration
public partial class Program;
