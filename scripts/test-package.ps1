param(
    [Parameter(Mandatory)]
    [string]$PackageDirectory,
    [Parameter(Mandatory)]
    [string]$Version,
    [ValidateSet('net8.0-windows', 'net462')]
    [string]$TargetFramework = 'net8.0-windows',
    [ValidateSet('x86', 'x64')]
    [string]$Platform = 'x64'
)

$ErrorActionPreference = 'Stop'
$feed = (Resolve-Path -LiteralPath $PackageDirectory).Path
$package = Join-Path $feed "Gosf.Input.Automation.$Version.nupkg"
function Assert-PackageContents {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        foreach ($required in @('runtimes/win-x86/native/Gosf.Input.Automation.Native.x86.dll',
            'runtimes/win-x64/native/Gosf.Input.Automation.Native.x64.dll', 'README.md', 'LICENSE',
            'lib/net462/Gosf.Input.Automation.dll', 'lib/net8.0-windows7.0/Gosf.Input.Automation.dll',
            'build/net462/Gosf.Input.Automation.targets', 'buildTransitive/net462/Gosf.Input.Automation.targets',
            'CHANGELOG.md', 'THIRD-PARTY-NOTICES.md', 'licenses/wow64ext-lgpl-3.0.txt', 'docs/NATIVE-REBUILD.md')) {
            if (-not $zip.GetEntry($required)) { throw "Package entry missing: $required" }
        }
    }
    finally { $zip.Dispose() }
}

function Write-ConsumerProject([string]$Consumer) {
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>$TargetFramework</TargetFramework>
    <LangVersion>12.0</LangVersion><Nullable>enable</Nullable><PlatformTarget>$Platform</PlatformTarget>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup><PackageReference Include="Gosf.Input.Automation" Version="$Version" /></ItemGroup>
  <ItemGroup Condition="'`$(TargetFramework)' == 'net462'">
    <Reference Include="System.Drawing" /><Reference Include="System.Windows.Forms" />
    <PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies" Version="1.0.3" PrivateAssets="all" />
  </ItemGroup>
</Project>
"@ | Set-Content (Join-Path $Consumer 'Consumer.csproj') -Encoding utf8
}

function Write-ConsumerSource([string]$Consumer) {
    if ($TargetFramework -eq 'net462') {
        Copy-Item (Join-Path $PSScriptRoot '../tests/Gosf.Input.Automation.FrameworkTests/Program.cs') (Join-Path $Consumer 'Program.cs')
        return
    }
@'
using Gosf.Input.Automation;
using var input = new InputSession();
if (input.IsBound || InputSession.BackendVersion != "0.4.8.3")
    throw new System.InvalidOperationException("Packaged backend smoke test failed.");
System.Console.WriteLine("PASS: NuGet package consumer loaded the native backend.");
'@ | Set-Content (Join-Path $Consumer 'Program.cs') -Encoding utf8
}

function Write-ConsumerNuGetConfig([string]$Consumer) {
    $escapedFeed = [Security.SecurityElement]::Escape($feed)
@"
<configuration>
  <packageSources><clear /><add key="local" value="$escapedFeed" /><add key="nuget" value="https://api.nuget.org/v3/index.json" /></packageSources>
  <packageSourceMapping>
    <packageSource key="local"><package pattern="Gosf.Input.Automation" /></packageSource>
    <packageSource key="nuget"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
"@ | Set-Content (Join-Path $Consumer 'NuGet.Config') -Encoding utf8
}

function Test-Consumer([string]$Consumer) {
    $project = Join-Path $Consumer 'Consumer.csproj'
    # Isolated packages plus source mapping ensure this exact local package is tested.
    & dotnet restore $project --configfile (Join-Path $Consumer 'NuGet.Config') --packages (Join-Path $Consumer 'packages')
    if ($LASTEXITCODE -ne 0) { throw 'Package consumer restore failed.' }
    & dotnet build $project -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Package consumer build failed.' }
    if ($TargetFramework -eq 'net462') {
        & (Join-Path $Consumer 'bin/Release/net462/Consumer.exe')
    } else {
        & dotnet run --project $project -c Release --no-build
    }
    if ($LASTEXITCODE -ne 0) { throw 'Package consumer regression failed.' }
}

Assert-PackageContents
$consumer = Join-Path ([IO.Path]::GetTempPath()) "gosf-package-test-$([guid]::NewGuid())"
New-Item -ItemType Directory -Path $consumer | Out-Null
Write-ConsumerProject $consumer
Write-ConsumerSource $consumer
Write-ConsumerNuGetConfig $consumer
Test-Consumer $consumer
