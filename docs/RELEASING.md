# Release guide

## One-time repository setup

1. Create the public GitHub repository and push the source. No repository URL is hard-coded: workflows derive package metadata from `github.repository`.
2. Confirm ownership/availability of the `Gosf.Input.Automation` package ID on nuget.org and review package author metadata.
3. On nuget.org, create a **Trusted Publishing** policy for the GitHub repository owner and repository name. Set **Workflow File** to `ci.yml` (the filename only). Leave **Environment** empty: this workflow does not use a GitHub Actions environment. Choose the policy owner and package scopes that allow publishing `Gosf.Input.Automation`, including creating the package if it has not been published yet. See [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).
4. The NuGet username defaults to the GitHub repository owner (`github.repository_owner`). If your nuget.org username differs, set `NUGET_USER` under GitHub **Settings → Secrets and variables → Actions → Variables** to override it (profile name, not email). An organization-level variable shared with this repository also works. For organization-owned repositories, the default is the organization name; override it when the NuGet login username differs. No long-lived API key secret is required.
5. Enable private vulnerability reporting in GitHub's security settings.

## Prepare a version

1. Set `Version` in `src/Gosf.Input.Automation/Gosf.Input.Automation.csproj`. Use `major.minor.patch` or `major.minor.patch-prerelease` (no build metadata).
2. Move completed `Unreleased` entries into a dated section for that version. The initial `0.1.0` is planned, not recorded as released.
3. Run CI (including net462 package regressions on x86 and x64) plus x86/x64 WPF regressions on an interactive Windows desktop. CI does not certify all desktop behavior. The net462 tests compile against 4.6.2 reference assemblies and run on the installed Framework 4.x runtime.
4. Review third-party notices and distribution terms, especially the statically linked LGPL WOW64Ext component. Check the source archive, retained notices, and successful rebuilding with modified component sources. Do not describe the entire binary package as MIT-only.
5. Commit all release changes. Publish a GitHub Release with tag `v<Version>`, such as `v0.1.0` or `v0.2.0-rc.1`, at that commit.

The publish workflow handles stable releases and prereleases via `release: published`. It validates the tag against the project version, rebuilds both native backends, runs WinForms regressions, packs NuGet, and creates a native source archive. Before publishing the NuGet package, it attaches the package, native sources, and SHA-256 checksums to the GitHub Release. This keeps corresponding source material available alongside the binary release.

Pull requests and pushes only build, test, and upload temporary CI artifacts. Manual CI runs provide a packaging dry run and never publish to nuget.org. Only the release publishing job has `id-token: write`. Immediately before pushing, `NuGet/login` exchanges its GitHub OIDC token for a short-lived publishing credential. No long-lived NuGet API key is stored in GitHub.

## Failed releases

Fix failures before retrying. NuGet versions are immutable: do not overwrite a published version with different content. The workflow deliberately does not use `--skip-duplicate`, so a duplicate version fails visibly. Existing release assets are never overwritten automatically. If uploading succeeded but NuGet publishing failed, verify that the version was not published before removing the failed run's assets and retrying. After a successful publish, inspect nuget.org package metadata and install the exact version in a separate x86/x64 application. Retain release source assets for as long as the corresponding binaries are distributed.
