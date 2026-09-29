$ErrorActionPreference = "Stop"

# Bump this when releasing, and keep it in step with <Version> in Recyclage.csproj.
# The GitHub Actions workflow reads the version from the csproj instead, so this
# value is only used when packing by hand.
$Version = "1.0.0"

$env:PATH = "$env:USERPROFILE\.dotnet\tools;$env:PATH"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "Recyclage.csproj"
$PublishDir = Join-Path $ProjectRoot "publish"
$ReleaseDir = Join-Path $ProjectRoot "releases"

Push-Location $ProjectRoot
try {
    # Remove stale output so a deleted source file cannot linger in the package.
    if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }
    if (Test-Path $ReleaseDir) { Remove-Item $ReleaseDir -Recurse -Force }

    dotnet publish $ProjectFile `
        -c Release `
        --self-contained `
        -r win-x64 `
        -o $PublishDir `
        /p:Version=$Version

    $IconPath = Join-Path $ProjectRoot "Assets\recyclage.ico"

    # packId / mainExe / icon must match Shared\Configuration\VelopackConfiguration.cs,
    # otherwise the installed app will not see the update feed.
    vpk pack `
        --packId Sonlighting.Recyclage `
        --packTitle "Recyclage" `
        --packVersion $Version `
        --packDir $PublishDir `
        --mainExe Recyclage.exe `
        --icon $IconPath `
        --outputDir $ReleaseDir
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Release artifacts written to $ReleaseDir"
Write-Host "Upload these to https://github.com/yassin-ajanif/recyclage/releases with tag v$Version :"
Write-Host "  Sonlighting.Recyclage-$Version-full.nupkg"
Write-Host "  Sonlighting.Recyclage-win-Setup.exe"
Write-Host "  releases.win.json"
Write-Host ""
Write-Host "Or let CI do it: push a commit that changes <Version> in Recyclage.csproj to main."
