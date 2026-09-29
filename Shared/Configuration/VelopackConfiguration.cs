namespace Recyclage.Shared.Configuration;

/// <summary>
/// Single source of truth for the Velopack identity. These values must match the
/// arguments passed to `vpk pack` in scripts/pack-release.ps1, and the values
/// baked into the published GitHub release.
/// </summary>
public static class VelopackConfiguration
{
    /// <summary>Unique, permanent app id. Changing this after a release breaks updates.</summary>
    public const string PackId = "Sonlighting.Recyclage";

    /// <summary>Name of the entry executable inside the published output.</summary>
    public const string MainExe = "Recyclage.exe";

    /// <summary>Repository the updater polls for new releases.</summary>
    public const string GitHubRepoUrl = "https://github.com/yassin-ajanif/recyclage";
}
