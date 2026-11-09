# Rebuilding the native backends

The release asset `Gosf.Input.Automation.<version>.native-sources.zip` contains the project sources, license texts, build scripts, and complete BlackBone and MinHook source directories used for the build. BlackBone includes AsmJit and WOW64Ext. `SOURCE-MANIFEST.txt` identifies dependency revisions and the project commit when available.

Download this asset from the same GitHub Release as the package version. NuGet metadata supplies the repository URL. GitHub's automatic source archives alone do not contain fetched dependencies.

## Build with bundled dependencies

On Windows, install Visual Studio 2022 with C++ tools and CMake. Extract the archive to a writable directory. Edit the component you want to change under `dependencies/`, then run:

```powershell
./scripts/build-native.ps1 -DependencySource ./dependencies
```

CMake uses these local dependency sources instead of fetching them. The C++ tools and Windows SDK must already be installed. Git and network access are not needed for dependency fetching in this mode. Use a fresh extraction/build directory when switching from downloaded to bundled dependencies.

The outputs are:

- `artifacts/gosf-input-automation-native-x86/Release/Gosf.Input.Automation.Native.x86.dll`
- `artifacts/gosf-input-automation-native-x64/Release/Gosf.Input.Automation.Native.x64.dll`

To rebuild the managed package as well, install the .NET 8 SDK and run:

```powershell
dotnet pack src/Gosf.Input.Automation/Gosf.Input.Automation.csproj -c Release -o artifacts/packages
```

Alternatively, replace the matching native DLL in your application's output directory with your rebuilt DLL. Preserve the filename and architecture. The managed library resolves the DLL according to the calling process architecture. Close the caller and target processes first so that no original or injected module remains loaded.

The project-owned MIT code permits modification and rebuilding; dependency terms are in `licenses/` and the bundled dependency trees. See [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md). Providing these materials is not a claim that all distribution obligations have been independently audited.
