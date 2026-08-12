using S1Lua.Generator;

namespace S1Lua.Tests;

public sealed class SurfaceGeneratorTests
{
    [Fact]
    public void RejectsMissingS1ApiAnchor()
    {
        string root = CreateTemporaryDirectory();
        try
        {
            string api = Path.Combine(root, "api");
            Directory.CreateDirectory(api);
            File.WriteAllText(Path.Combine(api, "fixture.yml"), "### YamlMime:ManagedReference\nitems:\n- uid: S1API.Exists\n");
            var surface = new SurfaceDefinition
            {
                SchemaVersion = 1,
                SurfaceVersion = "1.0.0",
                S1ApiVersion = "1.0.0",
                Bindings = new[]
                {
                    new BindingDefinition
                    {
                        Path = "s1.test",
                        Scope = "global",
                        Handler = "Test",
                        Signature = "s1.test()",
                        Summary = "Test binding.",
                        S1ApiUids = new[] { "S1API.Missing" }
                    }
                }
            };

            IReadOnlyList<string> errors = new SurfaceGenerator().Validate(surface, api);

            Assert.Contains(errors, error => error.Contains("S1API.Missing", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GeneratesAllMaintenanceArtifactsFromOneSurface()
    {
        var surface = new SurfaceDefinition
        {
            SchemaVersion = 1,
            SurfaceVersion = "1.2.3",
            S1ApiVersion = "3.1.12",
            Bindings = new[]
            {
                new BindingDefinition
                {
                    Path = "s1.log",
                    Scope = "global",
                    Handler = "Log",
                    Signature = "s1.log(message)",
                    Summary = "Writes a message.",
                    Parameters = new[]
                    {
                        new FieldDefinition { Name = "message", Type = "string", Required = true, Description = "Message." },
                        new FieldDefinition { Name = "prefix", Type = "string", Required = false, Description = "Optional prefix." }
                    }
                }
            }
        };

        IReadOnlyList<GeneratedArtifact> artifacts = new SurfaceGenerator().Generate(surface);

        Assert.Equal(4, artifacts.Count);
        Assert.Contains(artifacts, artifact => artifact.RelativePath.EndsWith("GeneratedSurface.g.cs", StringComparison.Ordinal));
        Assert.Contains(artifacts, artifact => artifact.RelativePath == "generated/s1lua.lua");
        Assert.Contains(artifacts, artifact => artifact.RelativePath == "docs/reference.md");
        Assert.Contains(artifacts, artifact => artifact.RelativePath == "generated/surface.snapshot.json");
        Assert.All(artifacts, artifact => Assert.Contains("1.2.3", artifact.Content, StringComparison.Ordinal));
        GeneratedArtifact luaStub = Assert.Single(artifacts, artifact => artifact.RelativePath == "generated/s1lua.lua");
        Assert.Contains("---@param prefix? string", luaStub.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void UpstreamUpdateBumpsPatchAndPreservesTheCuratedSurface()
    {
        string root = CreateTemporaryDirectory();
        try
        {
            string surfacePath = Path.Combine(root, "surface.json");
            File.WriteAllText(
                surfacePath,
                """
                {
                  "schemaVersion": 1,
                  "surfaceVersion": "0.1.9",
                  "s1ApiVersion": "3.1.12",
                  "summary": "Keep me",
                  "bindings": []
                }
                """);

            string version = new SurfaceGenerator().UpdateVersions(surfacePath, "3.1.13", bumpSurfacePatch: true);
            SurfaceDefinition updated = new SurfaceGenerator().Load(surfacePath);

            Assert.Equal("0.1.10", version);
            Assert.Equal("0.1.10", updated.SurfaceVersion);
            Assert.Equal("3.1.13", updated.S1ApiVersion);
            Assert.Equal("Keep me", updated.Summary);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "S1Lua.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
