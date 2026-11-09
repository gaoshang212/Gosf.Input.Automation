param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$')]
    [string]$Version,
    [string]$DependencySource,
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not $DependencySource) { $DependencySource = Join-Path $root 'artifacts/native-x64-deps' }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $root 'artifacts/packages' }

function Copy-SourceTree([string]$Source, [string]$Destination) {
    if (-not (Test-Path -LiteralPath $Source)) { throw "Missing source directory: $Source" }
    & robocopy $Source $Destination /E /XD .git bin obj /XF .git /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Could not copy source directory: $Source" }
}

function Copy-ProjectSources([string]$Destination) {
    foreach ($directory in @('src', 'tests', 'scripts', 'docs', 'licenses')) {
        Copy-SourceTree (Join-Path $root $directory) (Join-Path $Destination $directory)
    }
    foreach ($file in @('Gosf.Input.Automation.sln', 'LICENSE', 'THIRD-PARTY-NOTICES.md',
        'README.md', 'README.zh-CN.md', 'CHANGELOG.md', 'CONTRIBUTING.md', 'SECURITY.md')) {
        Copy-Item -LiteralPath (Join-Path $root $file) -Destination $Destination
    }
}

function Copy-DependencySources([string]$Destination) {
    foreach ($name in @('blackbone', 'minhook')) {
        $source = Join-Path $DependencySource "$name-src"
        Copy-SourceTree $source (Join-Path $Destination "dependencies/$name")
    }
}

function Write-SourceManifest([string]$Destination) {
    $commit = $env:GITHUB_SHA
    if (-not $commit) { $commit = 'Local working tree; may contain uncommitted changes.' }
    $pins = Select-String -Path (Join-Path $root 'src/Gosf.Input.Automation.Native/CMakeLists.txt') -Pattern 'GIT_TAG '
    @("Package version: $Version", "Project revision: $commit", 'Dependency pins from CMakeLists.txt:',
        ($pins.Line -join "`n"), 'See docs/NATIVE-REBUILD.md for build instructions.') |
        Set-Content -LiteralPath (Join-Path $Destination 'SOURCE-MANIFEST.txt') -Encoding utf8
}

$stage = Join-Path ([IO.Path]::GetTempPath()) "gosf-native-sources-$([guid]::NewGuid())"
New-Item -ItemType Directory -Path $stage -Force | Out-Null
Copy-ProjectSources $stage
Copy-DependencySources $stage
Write-SourceManifest $stage
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$archive = Join-Path (Resolve-Path -LiteralPath $OutputDirectory).Path "Gosf.Input.Automation.$Version.native-sources.zip"
& tar -a -c -f $archive -C $stage .
if ($LASTEXITCODE -ne 0) { throw 'Creating the native source archive failed.' }
Write-Host "Native source archive: $archive"
# Keep staging sources under the OS temporary directory for inspection.
exit 0
