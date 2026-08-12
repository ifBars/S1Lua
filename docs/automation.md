# CI and release automation

S1Lua is designed so an S1API stable release produces one review task, not a manual rebuild checklist.

## Release flow

1. `sync S1API` checks the latest stable `ifBars/S1API` GitHub release every hour. It can also receive `repository_dispatch` or run manually.
2. When the version changes, it checks out that exact S1API tag and validates every curated API UID.
3. If compatible, it bumps the S1Lua patch version, regenerates all derived files, runs public tests, and opens a PR.
4. The PR builds S1Lua against the actual published S1API release DLLs for Mono and IL2CPP using private compile-reference repositories.
5. You review the generated diff and CI. Merging is the release approval.
6. The successful `main` validation artifacts are checksummed, tagged, and published as a GitHub release automatically.

If an S1API UID disappeared, the sync job fails before opening a misleading compatibility PR. That is the intended maintenance signal: update the one handwritten beginner adapter and its surface anchor, then rerun the workflow.

## Required S1Lua repository secrets

Add these before the first push to `main`:

| Secret | Purpose |
| --- | --- |
| `GAME_ASSEMBLIES_REPO` | `owner/repository` containing the Mono `Managed` compile assemblies. |
| `GAME_ASSEMBLIES_TOKEN` | Fine-grained token with read-only Contents access to that private repository. |
| `IL2CPP_ASSEMBLIES_REPO` | `owner/repository` containing `MelonLoader/Il2CppAssemblies`. |
| `IL2CPP_ASSEMBLIES_TOKEN` | Fine-grained token with read-only Contents access to that private repository. |
| `S1LUA_AUTOMATION_TOKEN` | Fine-grained token for S1Lua with Contents read/write and Pull requests read/write. |

The first four can use the same values already used by S1API's release workflows. The automation token is important because PRs created with the built-in `GITHUB_TOKEN` do not start another workflow run. Limit the token to the S1Lua repository.

No NuGet, game-store, Nexus, or Thunderstore secret is required. S1Lua downloads the public S1API GitHub release and publishes only to its own GitHub Releases page.

The package gate also verifies the runtime-specific MoonSharp asset: `net40-client` for the game's Mono runtime and `netstandard1.6` for IL2CPP/.NET 6. This prevents a compile-compatible but unloadable netstandard dependency from reaching Mono users.

## Recommended repository settings

Protect `main`, require PRs, and require these checks:

- `Generated surface and tests`;
- `Mono and IL2CPP release build`.

Allow the automation token to create branches and PRs, but do not grant it bypass rights. The generated compatibility PR should pass the same review and branch protection as a human PR.

External fork PRs run the public contract tests but skip private-assembly builds because GitHub does not expose repository secrets to forks. A maintainer branch or follow-up PR is required for the private dual-runtime gate.

## Optional immediate S1API notification

The hourly watcher requires no S1API changes. For immediate pickup, add a fine-grained `S1LUA_DISPATCH_TOKEN` secret to S1API with Contents write access limited to S1Lua, then add this final step after S1API publishes its GitHub release:

```yaml
- name: Notify S1Lua
  if: success() && steps.metadata.outputs.prerelease != 'true'
  env:
    GH_TOKEN: ${{ secrets.S1LUA_DISPATCH_TOKEN }}
    S1API_VERSION: ${{ steps.metadata.outputs.release_version }}
  run: |
    jq -n \
      --arg version "$S1API_VERSION" \
      '{event_type: "s1api_released", client_payload: {version: $version}}' \
      | gh api --method POST repos/ifBars/S1Lua/dispatches --input -
```

This is only a latency optimization; the scheduled watcher remains the fallback.

## Manual recovery

Run `sync S1API` with a stable version when a scheduled run was delayed. Run `validate` manually to recreate tested artifacts. The `release` workflow intentionally publishes only artifacts from a successful push validation on `main`, so manually validating an unreviewed branch cannot publish a release.
