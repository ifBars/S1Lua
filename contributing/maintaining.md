# Maintaining the surface

S1Lua is a curated beginner API, not an automatic export of every public S1API type. The generator owns repetitive registration, reference, editor, and compatibility artifacts while small handwritten adapters preserve beginner-friendly behavior.

## Update for a new S1API release

1. Check out the intended S1API release in the sibling `S1API` folder.
2. Change `s1ApiVersion` in `surface/s1lua.surface.json`.
3. Run `./scripts/Generate.ps1`.
4. Resolve only missing or intentionally changed S1API UIDs reported by the generator.
5. Build and test both runtimes with `./scripts/Validate.ps1`.
6. Perform one in-game smoke test on Mono and IL2CPP using the included Golden Cuke example.

The normal hosted path performs steps 1-5 automatically and presents them as a review PR. The in-game smoke remains the only intentionally human release check until a safe game-runner environment is connected.

The surface snapshot fingerprints every referenced S1API UID. A S1API update that preserves those UIDs should require only regeneration and dual-runtime validation.

## Add a Lua feature

A feature has three deliberate pieces:

1. Describe the function, option, or event in `surface/s1lua.surface.json` and anchor it to the exact public S1API UIDs it uses.
2. Implement the small semantic adapter in `BeginnerApiBindings` or the runtime S1API adapter. Do not expose S1API, CLR, or Unity objects to Lua.
3. Add tests for happy-path behavior, beginner-facing validation messages, isolation, and execution limits.

Run the generator after changing the surface. Do not hand-edit these files:

- `src/S1Lua.Core/Generated/GeneratedSurface.g.cs`;
- `generated/s1lua.lua`;
- `docs/api/reference.md`;
- `generated/surface.snapshot.json`.

## Surface design test

Accept a feature only when a first-time modder can use it without learning S1API lifecycle stages, Unity object ownership, runtime interop, or .NET type names. Prefer a small declaration with safe defaults over a one-to-one wrapper of a builder.

Keep advanced capabilities in C#/S1API. Expanding S1Lua until it becomes a second general-purpose S1API would increase maintenance and make the beginner experience worse.

## Compatibility policy

- One Lua surface is shared by Mono and IL2CPP.
- Runtime-specific behavior stays behind handwritten C# adapters.
- Each surface binding records its S1API API anchors.
- A missing anchor is a generation failure, not a warning.
- Generated files must be current in CI.
- Release archives never contain game assemblies, S1API itself, or local deployment files.

`scripts/Validate.ps1 -SkipRuntime` is useful on machines without game assemblies. It still checks generation drift and all host-independent tests, but it is not a release gate.
