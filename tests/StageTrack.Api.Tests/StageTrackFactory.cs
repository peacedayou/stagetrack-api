using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StageTrack.Api.Tests;

/// <summary>Démarre l'API en mémoire avec une base SQLite temporaire, propre à chaque test.</summary>
public class StageTrackFactory : WebApplicationFactory<Program>
{
    private readonly string _cheminBase = Path.Combine(Path.GetTempPath(), $"stagetrack-test-{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_cheminBase};Pooling=False");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(_cheminBase)) File.Delete(_cheminBase);
    }
}
