namespace S1Lua.Generator;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            GeneratorOptions options = ParseOptions(args);
            var generator = new SurfaceGenerator();
            if (options.UpdateS1ApiVersion != null)
            {
                if (options.Check)
                    throw new ArgumentException("--update-s1api cannot be combined with --check.");

                string version = generator.UpdateVersions(
                    options.SurfacePath,
                    options.UpdateS1ApiVersion,
                    options.BumpSurfacePatch);
                File.WriteAllText(Path.Combine(options.RepositoryRoot, "version.txt"), version + Environment.NewLine);
                Console.WriteLine($"updated: S1Lua {version} for S1API {options.UpdateS1ApiVersion}");
            }

            SurfaceDefinition surface = generator.Load(options.SurfacePath);
            var errors = generator.Validate(surface, options.S1ApiDirectory).ToList();
            string versionPath = Path.Combine(options.RepositoryRoot, "version.txt");
            if (!File.Exists(versionPath))
            {
                errors.Add($"Version file does not exist: {versionPath}");
            }
            else
            {
                string productVersion = File.ReadAllText(versionPath).Trim();
                if (!string.Equals(productVersion, surface.SurfaceVersion, StringComparison.Ordinal))
                    errors.Add($"version.txt ({productVersion}) does not match surfaceVersion ({surface.SurfaceVersion}).");
            }
            if (errors.Count > 0)
            {
                foreach (string error in errors)
                    Console.Error.WriteLine($"error: {error}");
                return 1;
            }

            IReadOnlyList<GeneratedArtifact> artifacts = generator.Generate(surface);
            bool changed = false;
            foreach (GeneratedArtifact artifact in artifacts)
            {
                string path = Path.Combine(options.RepositoryRoot, artifact.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                string normalized = NormalizeNewlines(artifact.Content);
                string? existing = File.Exists(path) ? NormalizeNewlines(File.ReadAllText(path)) : null;
                if (string.Equals(existing, normalized, StringComparison.Ordinal))
                    continue;

                changed = true;
                if (options.Check)
                {
                    Console.Error.WriteLine($"out of date: {artifact.RelativePath}");
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, normalized);
                Console.WriteLine($"generated: {artifact.RelativePath}");
            }

            if (options.Check && changed)
                return 2;

            Console.WriteLine(options.Check ? "Generated surface is current." : "Surface generation complete.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static GeneratorOptions ParseOptions(IReadOnlyList<string> args)
    {
        string repositoryRoot = Directory.GetCurrentDirectory();
        string? surfacePath = null;
        string? s1ApiDirectory = null;
        bool check = false;
        string? updateS1ApiVersion = null;
        bool bumpSurfacePatch = false;

        for (int index = 0; index < args.Count; index++)
        {
            string argument = args[index];
            switch (argument)
            {
                case "--check":
                    check = true;
                    break;
                case "--repo-root":
                    repositoryRoot = ReadValue(args, ref index, argument);
                    break;
                case "--surface":
                    surfacePath = ReadValue(args, ref index, argument);
                    break;
                case "--s1api-api":
                    s1ApiDirectory = ReadValue(args, ref index, argument);
                    break;
                case "--update-s1api":
                    updateS1ApiVersion = ReadValue(args, ref index, argument);
                    break;
                case "--bump-surface-patch":
                    bumpSurfacePatch = true;
                    break;
                case "-h":
                case "--help":
                    PrintUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {argument}");
            }
        }

        repositoryRoot = Path.GetFullPath(repositoryRoot);
        surfacePath ??= Path.Combine(repositoryRoot, "surface", "s1lua.surface.json");
        s1ApiDirectory ??= Path.GetFullPath(Path.Combine(repositoryRoot, "..", "S1API", "S1API", "api"));

        return new GeneratorOptions(
            Path.GetFullPath(surfacePath),
            Path.GetFullPath(s1ApiDirectory),
            repositoryRoot,
            check,
            updateS1ApiVersion,
            bumpSurfacePatch);
    }

    private static string ReadValue(IReadOnlyList<string> args, ref int index, string argument)
    {
        index++;
        if (index >= args.Count)
            throw new ArgumentException($"Missing value for {argument}.");
        return args[index];
    }

    private static string NormalizeNewlines(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    private static void PrintUsage()
    {
        Console.WriteLine("S1Lua.Generator [--check] [--repo-root PATH] [--surface PATH] [--s1api-api PATH]");
        Console.WriteLine("                [--update-s1api VERSION] [--bump-surface-patch]");
    }
}
