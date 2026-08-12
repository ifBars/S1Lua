namespace S1Lua.Runtime;

internal static class ModAssetPaths
{
    internal static string ResolvePng(string sourceDirectory, string relativePath, string label)
    {
        string root = Path.GetFullPath(sourceDirectory);
        string path = Path.GetFullPath(Path.Combine(root, relativePath));
        string rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                            Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"The {label} path must stay inside this mod's folder.");
        if (!string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"{label} files must be PNG images.");
        return path;
    }
}
