param(
    [ValidateSet('all', 'x86', 'x64')]
    [string]$Architecture = 'all',
    [string]$DependencySource
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vs = & $vswhere -version '[17.0,18.0)' -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath | Select-Object -First 1
if (-not $vs) { throw 'Visual Studio 2022 with the C++ build tools is required.' }

$cmake = Join-Path $vs 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe'
if (-not (Test-Path -LiteralPath $cmake)) {
    $command = Get-Command cmake.exe -ErrorAction Stop
    $cmake = $command.Source
}

$architectures = if ($Architecture -eq 'all') { @('x86', 'x64') } else { @($Architecture) }
$dependencyOptions = @()
if ($DependencySource) {
    $dependencyRoot = (Resolve-Path -LiteralPath $DependencySource).Path
    foreach ($dependency in @('blackbone', 'minhook')) {
        $source = Join-Path $dependencyRoot $dependency
        if (-not (Test-Path -LiteralPath "$source/CMakeLists.txt")) {
            # BlackBone keeps its CMake entry point under src/BlackBone.
            if ($dependency -ne 'blackbone' -or -not (Test-Path -LiteralPath "$source/src/BlackBone/CMakeLists.txt")) {
                throw "Missing $dependency sources in $source."
            }
        }
        $dependencyOptions += "-DFETCHCONTENT_SOURCE_DIR_$($dependency.ToUpperInvariant())=$source"
    }
}
foreach ($arch in $architectures) {
    $build = Join-Path $root "artifacts\gosf-input-automation-native-$arch"
    $platform = if ($arch -eq 'x86') { 'Win32' } else { 'x64' }
    & $cmake -S (Join-Path $root 'src\Gosf.Input.Automation.Native') -B $build `
        -G 'Visual Studio 17 2022' -A $platform `
        "-DFETCHCONTENT_BASE_DIR=$(Join-Path $root "artifacts\native-$arch-deps")" @dependencyOptions
    if ($LASTEXITCODE -ne 0) { throw "Native $arch configuration failed." }
    & $cmake --build $build --config Release --parallel 4
    if ($LASTEXITCODE -ne 0) { throw "Native $arch build failed." }
}

