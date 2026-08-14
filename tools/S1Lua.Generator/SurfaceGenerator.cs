using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace S1Lua.Generator;

public sealed class SurfaceGenerator
{
    private static readonly Regex StableVersionPattern = new(
        "^[0-9]+\\.[0-9]+\\.[0-9]+$",
        RegexOptions.CultureInvariant);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    public SurfaceDefinition Load(string surfacePath)
    {
        string json = File.ReadAllText(surfacePath);
        SurfaceDefinition? surface = JsonSerializer.Deserialize<SurfaceDefinition>(json, JsonOptions);
        return surface ?? throw new InvalidOperationException($"Could not parse surface definition: {surfacePath}");
    }

    public string UpdateVersions(string surfacePath, string s1ApiVersion, bool bumpSurfacePatch)
    {
        if (!StableVersionPattern.IsMatch(s1ApiVersion))
            throw new ArgumentException($"S1API version '{s1ApiVersion}' must be a stable major.minor.patch version.");

        JsonNode? node = JsonNode.Parse(File.ReadAllText(surfacePath));
        if (node is not JsonObject root)
            throw new InvalidOperationException($"Could not parse surface definition: {surfacePath}");

        string surfaceVersion = root["surfaceVersion"]?.GetValue<string>()
                                ?? throw new InvalidOperationException("surfaceVersion is required.");
        if (bumpSurfacePatch)
            surfaceVersion = IncrementPatchVersion(surfaceVersion);

        root["surfaceVersion"] = surfaceVersion;
        root["s1ApiVersion"] = s1ApiVersion;
        File.WriteAllText(surfacePath, root.ToJsonString(JsonOptions) + Environment.NewLine);
        return surfaceVersion;
    }

    public static string IncrementPatchVersion(string version)
    {
        if (!StableVersionPattern.IsMatch(version))
            throw new ArgumentException($"Surface version '{version}' must be a stable major.minor.patch version.");

        string[] components = version.Split('.');
        int patch = checked(int.Parse(components[2], System.Globalization.CultureInfo.InvariantCulture) + 1);
        return $"{components[0]}.{components[1]}.{patch}";
    }

    public IReadOnlyList<string> Validate(SurfaceDefinition surface, string s1ApiDirectory)
    {
        var errors = new List<string>();
        if (surface.SchemaVersion != 1)
            errors.Add($"Unsupported schemaVersion {surface.SchemaVersion}; expected 1.");
        if (string.IsNullOrWhiteSpace(surface.SurfaceVersion))
            errors.Add("surfaceVersion is required.");
        if (string.IsNullOrWhiteSpace(surface.S1ApiVersion))
            errors.Add("s1ApiVersion is required.");

        AddDuplicates(surface.Types.Select(type => type.Name), "type", errors);
        AddDuplicates(surface.Bindings.Select(binding => binding.Path), "binding", errors);
        AddDuplicates(surface.Events.Select(evt => evt.Name), "event", errors);

        foreach (SurfaceTypeDefinition type in surface.Types)
        {
            if (string.IsNullOrWhiteSpace(type.Name))
                errors.Add("Every type needs a name.");
            else if (type.Name.StartsWith("S1Lua", StringComparison.Ordinal))
                errors.Add($"Lua type {type.Name} must not repeat the S1Lua product prefix.");
            AddDuplicates(type.Fields.Select(field => field.Name), $"field in {type.Name}", errors);
            foreach (FieldDefinition field in type.Fields)
                ValidateField(field, $"{type.Name}.{field.Name}", errors);
        }

        foreach (BindingDefinition binding in surface.Bindings)
        {
            if (binding.Scope is not ("global" or "mod" or "npc" or "quest"))
                errors.Add($"Binding {binding.Path} has unsupported scope '{binding.Scope}'.");
            string expectedPrefix = binding.Scope == "global" ? "s1." : $"{binding.Scope}.";
            if (!binding.Path.StartsWith(expectedPrefix, StringComparison.Ordinal))
                errors.Add($"Binding {binding.Path} does not match its {binding.Scope} scope.");
            if (string.IsNullOrWhiteSpace(binding.Handler))
                errors.Add($"Binding {binding.Path} needs a handler.");
            if (string.IsNullOrWhiteSpace(binding.Signature) || string.IsNullOrWhiteSpace(binding.Summary))
                errors.Add($"Binding {binding.Path} needs a signature and summary.");
            AddDuplicates(binding.Parameters.Select(parameter => parameter.Name), $"parameter in {binding.Path}", errors);
            foreach (FieldDefinition parameter in binding.Parameters)
                ValidateField(parameter, $"{binding.Path}({parameter.Name})", errors);
        }

        foreach (EventDefinition evt in surface.Events)
        {
            if (string.IsNullOrWhiteSpace(evt.Name) || string.IsNullOrWhiteSpace(evt.Summary))
                errors.Add("Every event needs a name and summary.");
            AddDuplicates(evt.Parameters.Select(parameter => parameter.Name), $"callback parameter in {evt.Name}", errors);
            foreach (FieldDefinition parameter in evt.Parameters)
                ValidateField(parameter, $"{evt.Name}({parameter.Name})", errors);
        }

        if (!Directory.Exists(s1ApiDirectory))
        {
            errors.Add($"S1API DocFX directory does not exist: {s1ApiDirectory}");
            return errors;
        }

        HashSet<string> catalog = ReadS1ApiUids(s1ApiDirectory);
        foreach (string uid in ReferencedUids(surface).Distinct(StringComparer.Ordinal))
        {
            if (!catalog.Contains(uid))
                errors.Add($"S1API {surface.S1ApiVersion} does not contain required UID: {uid}");
        }

        return errors;
    }

    public IReadOnlyList<GeneratedArtifact> Generate(SurfaceDefinition surface)
    {
        return new[]
        {
            new GeneratedArtifact("src/S1Lua.Core/Generated/GeneratedSurface.g.cs", GenerateCSharp(surface)),
            new GeneratedArtifact("generated/s1lua.lua", GenerateLuaStub(surface)),
            new GeneratedArtifact("docs/api/reference.md", GenerateReference(surface)),
            new GeneratedArtifact("generated/surface.snapshot.json", GenerateSnapshot(surface))
        };
    }

    public static HashSet<string> ReadS1ApiUids(string directory)
    {
        var uids = new HashSet<string>(StringComparer.Ordinal);
        foreach (string file in Directory.EnumerateFiles(directory, "*.yml", SearchOption.AllDirectories))
        {
            foreach (string line in File.ReadLines(file))
            {
                string trimmed = line.TrimStart();
                const string prefix = "- uid: ";
                if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                    uids.Add(trimmed[prefix.Length..].Trim());
            }
        }

        return uids;
    }

    private static IEnumerable<string> ReferencedUids(SurfaceDefinition surface)
    {
        foreach (BindingDefinition binding in surface.Bindings)
        {
            foreach (string uid in binding.S1ApiUids)
                yield return uid;
        }

        foreach (EventDefinition evt in surface.Events)
            yield return evt.S1ApiUid;
    }

    private static void AddDuplicates(IEnumerable<string> values, string label, ICollection<string> errors)
    {
        foreach (IGrouping<string, string> duplicate in values
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .GroupBy(value => value, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1))
        {
            errors.Add($"Duplicate {label}: {duplicate.Key}");
        }
    }

    private static void ValidateField(FieldDefinition field, string location, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(field.Name) || string.IsNullOrWhiteSpace(field.Type))
            errors.Add($"{location} needs a name and type.");
        if (string.IsNullOrWhiteSpace(field.Description))
            errors.Add($"{location} needs a description.");
    }

    private static string GenerateCSharp(SurfaceDefinition surface)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("using MoonSharp.Interpreter;");
        builder.AppendLine("using S1Lua.Scripting;");
        builder.AppendLine();
        builder.AppendLine("namespace S1Lua.Generated;");
        builder.AppendLine();
        builder.AppendLine("public static class S1LuaBuild");
        builder.AppendLine("{");
        builder.AppendLine($"    public const string Version = \"{EscapeCSharp(surface.SurfaceVersion)}\";");
        builder.AppendLine($"    public const string S1ApiVersion = \"{EscapeCSharp(surface.S1ApiVersion)}\";");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("internal static class GeneratedSurface");
        builder.AppendLine("{");
        builder.AppendLine("    internal const string Version = S1LuaBuild.Version;");
        builder.AppendLine("    internal const string S1ApiVersion = S1LuaBuild.S1ApiVersion;");
        builder.AppendLine();
        builder.AppendLine("    internal static readonly SurfaceBindingInfo[] Bindings =");
        builder.AppendLine("    {");
        foreach (BindingDefinition binding in surface.Bindings)
        {
            builder.AppendLine(
                $"        new(\"{EscapeCSharp(binding.Path)}\", \"{EscapeCSharp(binding.Signature)}\", \"{EscapeCSharp(binding.Summary)}\"),");
        }
        builder.AppendLine("    };");
        builder.AppendLine();
        builder.AppendLine("    internal static void RegisterGlobals(Script script, BeginnerApiBindings bindings)");
        builder.AppendLine("    {");
        builder.AppendLine("        var api = new Table(script);");
        foreach (BindingDefinition binding in surface.Bindings.Where(binding => binding.Scope == "global"))
        {
            string name = binding.Path[(binding.Path.IndexOf('.') + 1)..];
            builder.AppendLine($"        api.Set(\"{EscapeCSharp(name)}\", DynValue.NewCallback(bindings.{binding.Handler}));");
        }
        builder.AppendLine("        script.Globals.Set(\"s1\", DynValue.NewTable(api));");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    internal static void RegisterMod(Table table, BeginnerApiBindings bindings, ScriptModSession session)");
        builder.AppendLine("    {");
        foreach (BindingDefinition binding in surface.Bindings.Where(binding => binding.Scope == "mod"))
        {
            string name = binding.Path[(binding.Path.IndexOf('.') + 1)..];
            builder.AppendLine($"        table.Set(\"{EscapeCSharp(name)}\", DynValue.NewCallback((context, args) => bindings.{binding.Handler}(session, context, args)));");
        }
        builder.AppendLine("    }");
        builder.AppendLine();
        AppendProxyRegistration(builder, surface, "npc", "Npc");
        AppendProxyRegistration(builder, surface, "quest", "Quest");
        builder.AppendLine("    internal static bool IsKnownEvent(string name)");
        builder.AppendLine("    {");
        builder.AppendLine("        return name switch");
        builder.AppendLine("        {");
        foreach (EventDefinition evt in surface.Events)
            builder.AppendLine($"            \"{EscapeCSharp(evt.Name)}\" => true,");
        builder.AppendLine("            _ => false");
        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void AppendProxyRegistration(
        StringBuilder builder,
        SurfaceDefinition surface,
        string scope,
        string methodSuffix)
    {
        builder.AppendLine(
            $"    internal static void Register{methodSuffix}(Table table, BeginnerApiBindings bindings, ScriptModSession session, string targetId)");
        builder.AppendLine("    {");
        foreach (BindingDefinition binding in surface.Bindings.Where(binding => binding.Scope == scope))
        {
            string name = binding.Path[(binding.Path.IndexOf('.') + 1)..];
            builder.AppendLine(
                $"        table.Set(\"{EscapeCSharp(name)}\", DynValue.NewCallback((context, args) => bindings.{binding.Handler}(session, targetId, context, args)));");
        }
        builder.AppendLine("    }");
        builder.AppendLine();
    }

    private static string GenerateLuaStub(SurfaceDefinition surface)
    {
        var builder = new StringBuilder();
        builder.AppendLine("---@meta");
        builder.AppendLine("-- Generated from surface/s1lua.surface.json. Do not edit by hand.");
        builder.AppendLine($"-- Surface version {surface.SurfaceVersion}; S1API {surface.S1ApiVersion}.");
        builder.AppendLine();
        builder.AppendLine("---A supported S1Lua game event. Type a quote after mod:on( to see every choice.");
        builder.AppendLine("---@alias S1EventName");
        foreach (EventDefinition evt in surface.Events)
            builder.AppendLine($"---| '\"{evt.Name}\"' # {evt.Summary}");
        builder.AppendLine();
        foreach (SurfaceTypeDefinition type in surface.Types)
        {
            builder.AppendLine($"---{type.Summary}");
            builder.AppendLine($"---@class {type.Name}");
            foreach (FieldDefinition field in type.Fields)
            {
                string optional = field.Required ? string.Empty : "?";
                builder.AppendLine($"---@field {field.Name}{optional} {field.Type} {field.Description}");
            }
            builder.AppendLine();
        }

        builder.AppendLine("---@class Mod");
        builder.AppendLine("local mod = {}");
        builder.AppendLine();
        builder.AppendLine("---@class Npc");
        builder.AppendLine("local npc = {}");
        builder.AppendLine();
        builder.AppendLine("---@class Quest");
        builder.AppendLine("local quest = {}");
        builder.AppendLine();
        builder.AppendLine("---@class Api");
        builder.AppendLine("local s1 = {}");
        builder.AppendLine();

        foreach (BindingDefinition binding in surface.Bindings)
        {
            builder.AppendLine($"---{binding.Summary}");
            if (binding.Path == "mod.on")
            {
                foreach (EventDefinition evt in surface.Events)
                {
                    builder.AppendLine(
                        $"---@overload fun(event: \"{evt.Name}\", callback: {GenerateCallbackType(evt)})");
                }
            }
            foreach (FieldDefinition parameter in binding.Parameters)
            {
                string optional = parameter.Required ? string.Empty : "?";
                builder.AppendLine($"---@param {parameter.Name}{optional} {parameter.Type}");
            }
            if (!string.IsNullOrWhiteSpace(binding.Returns))
                builder.AppendLine($"---@return {binding.Returns}");

            string method = binding.Path[(binding.Path.IndexOf('.') + 1)..];
            string parameters = string.Join(", ", binding.Parameters.Select(parameter => parameter.Name));
            string owner = binding.Scope == "global" ? "s1" : binding.Scope;
            string separator = binding.Scope == "global" ? "." : ":";
            builder.AppendLine($"function {owner}{separator}{method}({parameters}) end");
            builder.AppendLine();
        }

        builder.AppendLine("_G.s1 = s1");
        builder.AppendLine("return s1");
        return builder.ToString();
    }

    private static string GenerateReference(SurfaceDefinition surface)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!-- Generated from surface/s1lua.surface.json. Do not edit by hand. -->");
        builder.AppendLine("# S1Lua reference");
        builder.AppendLine();
        builder.AppendLine($"Surface version `{surface.SurfaceVersion}` for S1API `{surface.S1ApiVersion}`.");
        builder.AppendLine();
        builder.AppendLine(surface.Summary);
        builder.AppendLine();
        builder.AppendLine("This page lists every function and option currently available in S1Lua. If something is not listed here, it is not supported yet.");
        builder.AppendLine();
        builder.AppendLine("## Functions");
        builder.AppendLine();
        var bindingGroups = new[]
        {
            (Scope: "global", Title: "Global API"),
            (Scope: "mod", Title: "Mod API"),
            (Scope: "npc", Title: "NPC functions"),
            (Scope: "quest", Title: "Quest functions")
        };
        foreach ((string scope, string title) in bindingGroups)
        {
            BindingDefinition[] bindings = surface.Bindings
                .Where(binding => binding.Scope == scope)
                .ToArray();
            if (bindings.Length == 0)
                continue;

            builder.AppendLine($"### {title}");
            builder.AppendLine();
            foreach (BindingDefinition binding in bindings)
            {
                builder.AppendLine($"#### `{binding.Signature}`");
                builder.AppendLine();
                builder.AppendLine(binding.Summary);
                builder.AppendLine();
                if (binding.Parameters.Count > 0)
                {
                    builder.AppendLine("| Parameter | Type | Required | Description |");
                    builder.AppendLine("| --- | --- | --- | --- |");
                    foreach (FieldDefinition parameter in binding.Parameters)
                        builder.AppendLine($"| `{parameter.Name}` | `{parameter.Type}` | {(parameter.Required ? "yes" : "no")} | {parameter.Description} |");
                    builder.AppendLine();
                }
                foreach (string example in binding.Examples)
                {
                    builder.AppendLine("```lua");
                    builder.AppendLine(example);
                    builder.AppendLine("```");
                    builder.AppendLine();
                }
            }
        }

        builder.AppendLine("## Events");
        builder.AppendLine();
        builder.AppendLine("| Event | Callback values | When it runs |");
        builder.AppendLine("| --- | --- | --- |");
        foreach (EventDefinition evt in surface.Events)
        {
            string callback = evt.Parameters.Count == 0
                ? "none"
                : string.Join(", ", evt.Parameters.Select(parameter => $"`{parameter.Name}: {parameter.Type}`"));
            builder.AppendLine($"| `{evt.Name}` | {callback} | {evt.Summary} |");
        }
        builder.AppendLine();
        builder.AppendLine("## Option tables");
        builder.AppendLine();
        foreach (SurfaceTypeDefinition type in surface.Types)
        {
            builder.AppendLine($"### `{type.Name}`");
            builder.AppendLine();
            builder.AppendLine(type.Summary);
            builder.AppendLine();
            builder.AppendLine("| Field | Type | Required | Default | Description |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (FieldDefinition field in type.Fields)
            {
                builder.AppendLine(
                    $"| `{field.Name}` | `{field.Type}` | {(field.Required ? "yes" : "no")} | {field.Default ?? "-"} | {field.Description} |");
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }

    private static string GenerateSnapshot(SurfaceDefinition surface)
    {
        string[] uids = ReferencedUids(surface).Distinct(StringComparer.Ordinal).OrderBy(uid => uid, StringComparer.Ordinal).ToArray();
        string fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", uids)))).ToLowerInvariant();
        var snapshot = new
        {
            surfaceVersion = surface.SurfaceVersion,
            s1ApiVersion = surface.S1ApiVersion,
            bindingCount = surface.Bindings.Count,
            eventCount = surface.Events.Count,
            referencedUidFingerprint = fingerprint,
            bindings = surface.Bindings.Select(binding => binding.Path).OrderBy(path => path, StringComparer.Ordinal),
            events = surface.Events.Select(evt => evt.Name).OrderBy(name => name, StringComparer.Ordinal),
            eventSignatures = surface.Events
                .OrderBy(evt => evt.Name, StringComparer.Ordinal)
                .Select(evt => new
                {
                    evt.Name,
                    parameters = evt.Parameters.Select(parameter => new { parameter.Name, parameter.Type })
                }),
            referencedUids = uids
        };
        return JsonSerializer.Serialize(snapshot, JsonOptions) + Environment.NewLine;
    }

    private static string EscapeCSharp(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string GenerateCallbackType(EventDefinition evt)
    {
        string parameters = string.Join(", ", evt.Parameters.Select(parameter => $"{parameter.Name}: {parameter.Type}"));
        return $"fun({parameters})";
    }
}
