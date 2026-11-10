using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StageTrack.Api.Tests;

public class ApiTests : IDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly StageTrackFactory _factory = new();
    private readonly HttpClient _client;

    public ApiTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    private record Entreprise(int Id, string Nom, int NombreCandidatures);
    private record Candidature(int Id, string Poste, string Statut, DateOnly? DateEnvoi, string Entreprise);
    private record Stats(int Total, Dictionary<string, int> ParStatut, double TauxEntrevue);

    private async Task<Entreprise> CreerEntreprise(string nom)
    {
        var reponse = await _client.PostAsJsonAsync("/api/entreprises", new { nom, ville = "Lévis" });
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        return (await reponse.Content.ReadFromJsonAsync<Entreprise>(Json))!;
    }

    private async Task<Candidature> CreerCandidature(int entrepriseId, string poste)
    {
        var reponse = await _client.PostAsJsonAsync("/api/candidatures", new { poste, entrepriseId });
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        return (await reponse.Content.ReadFromJsonAsync<Candidature>(Json))!;
    }

    [Fact]
    public async Task Creer_une_candidature_la_retourne_avec_le_nom_de_l_entreprise()
    {
        var entreprise = await CreerEntreprise("Desjardins");

        var candidature = await CreerCandidature(entreprise.Id, "Stagiaire développeur");

        Assert.Equal("Desjardins", candidature.Entreprise);
        Assert.Equal("AEnvoyer", candidature.Statut);
    }

    [Fact]
    public async Task Entreprise_en_double_retourne_409()
    {
        await CreerEntreprise("iA Groupe financier");

        var reponse = await _client.PostAsJsonAsync("/api/entreprises", new { nom = "iA Groupe financier" });

        Assert.Equal(HttpStatusCode.Conflict, reponse.StatusCode);
    }

    [Fact]
    public async Task Candidature_sans_poste_retourne_400()
    {
        var entreprise = await CreerEntreprise("Beenox");

        var reponse = await _client.PostAsJsonAsync("/api/candidatures", new { poste = "", entrepriseId = entreprise.Id });

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Candidature_pour_entreprise_inexistante_retourne_400()
    {
        var reponse = await _client.PostAsJsonAsync("/api/candidatures", new { poste = "Stagiaire", entrepriseId = 999 });

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Passer_a_Envoyee_fixe_la_date_d_envoi()
    {
        var entreprise = await CreerEntreprise("Exfo");
        var candidature = await CreerCandidature(entreprise.Id, "Stagiaire C#");

        var reponse = await _client.PatchAsJsonAsync($"/api/candidatures/{candidature.Id}/statut", new { statut = "Envoyee" });
        var miseAJour = await reponse.Content.ReadFromJsonAsync<Candidature>(Json);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Equal("Envoyee", miseAJour!.Statut);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), miseAJour.DateEnvoi);
    }

    [Fact]
    public async Task Filtrer_par_statut()
    {
        var entreprise = await CreerEntreprise("Vanderlande");
        var a = await CreerCandidature(entreprise.Id, "Stage A");
        await CreerCandidature(entreprise.Id, "Stage B");
        await _client.PatchAsJsonAsync($"/api/candidatures/{a.Id}/statut", new { statut = "Entrevue" });

        var resultat = await _client.GetFromJsonAsync<List<Candidature>>("/api/candidatures?statut=Entrevue", Json);

        Assert.Single(resultat!);
        Assert.Equal("Stage A", resultat![0].Poste);
    }

    [Fact]
    public async Task Statistiques_calcule_le_taux_d_entrevue()
    {
        var entreprise = await CreerEntreprise("Giro");
        var a = await CreerCandidature(entreprise.Id, "Stage 1");
        var b = await CreerCandidature(entreprise.Id, "Stage 2");
        await CreerCandidature(entreprise.Id, "Stage 3"); // reste « À envoyer »
        await _client.PatchAsJsonAsync($"/api/candidatures/{a.Id}/statut", new { statut = "Envoyee" });
        await _client.PatchAsJsonAsync($"/api/candidatures/{b.Id}/statut", new { statut = "Entrevue" });

        var stats = await _client.GetFromJsonAsync<Stats>("/api/statistiques", Json);

        Assert.Equal(3, stats!.Total);
        Assert.Equal(1, stats.ParStatut["Entrevue"]);
        Assert.Equal(50.0, stats.TauxEntrevue); // 1 entrevue sur 2 candidatures envoyées
    }

    [Fact]
    public async Task Supprimer_une_entreprise_supprime_ses_candidatures()
    {
        var entreprise = await CreerEntreprise("Canam");
        await CreerCandidature(entreprise.Id, "Soutien TI");

        var reponse = await _client.DeleteAsync($"/api/entreprises/{entreprise.Id}");
        var restantes = await _client.GetFromJsonAsync<List<Candidature>>("/api/candidatures", Json);

        Assert.Equal(HttpStatusCode.NoContent, reponse.StatusCode);
        Assert.Empty(restantes!);
    }

    [Fact]
    public async Task Page_d_accueil_et_health_repondent()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health")).StatusCode);
    }
}
