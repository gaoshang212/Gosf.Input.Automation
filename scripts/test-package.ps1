param(
    [Parameter(Mandatory)]
    [string]$PackageDirectory,
    [Parameter(Mandatory)]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$feed = (Resolve-Path -LiteralPath $PackageDirectory).Path
$package = Join-Path $feed "Gosf.Input.Automation.$Version.nupkg"
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($package)
try {
    foreach ($required in @('runtimes/win-x86/native/Gosf.Input.Automation.Native.x86.dll',
        'runtimes/win-x64/native/Gosf.Input.Automation.Native.x64.dll', 'README.md', 'LICENSE',
        'CHANGELOG.md', 'THIRD-PARTY-NOTICES.md', 'licenses/wow64ext-lgpl-3.0.txt', 'docs/NATIVE-REBUILD.md')) {
        if (-not $zip.GetEntry($required)) { throw "Package entry missing: $required" }
    }
}
finally { $zip.Dispose() }

$consumer = Join-Path ([IO.Path]::GetTempPath()) "gosf-package-test-$([guid]::NewGuid())"
New-Item -ItemType Directory -Path $consumer | Out-Null
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0-windows</TargetFramework></PropertyGroup>
  <ItemGroup><PackageReference Include="Gosf.Input.Automation" Version="$Version" /></ItemGroup>
</Project>
"@ | Set-Content (Join-Path $consumer 'Consumer.csproj') -Encoding utf8
@'
using Gosf.Input.Automation;
using var input = new InputSession();
if (input.IsBound || InputSession.BackendVersion != "0.4.8.3")
    throw new System.InvalidOperationException("Packaged backend smoke test failed.");
System.Console.WriteLine("PASS: NuGet package consumer loaded the native backend.");
'@ | Set-Content (Join-Path $consumer 'Program.cs') -Encoding utf8
# A fresh cache ensures this exact package is tested even if the version already exists locally.
& dotnet restore (Join-Path $consumer 'Consumer.csproj') --source $feed --packages (Join-Path $consumer 'packages')
if ($LASTEXITCODE -ne 0) { throw 'Package consumer restore failed.' }
& dotnet run --project (Join-Path $consumer 'Consumer.csproj') -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Package consumer smoke test failed.' }
