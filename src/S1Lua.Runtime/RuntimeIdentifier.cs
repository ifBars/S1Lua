using System.Text;

namespace S1Lua.Runtime;

internal static class RuntimeIdentifier
{
    internal static string FromPascalCase(string value)
    {
        var result = new StringBuilder(value.Length + 2);
        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            if (index > 0 && char.IsUpper(character))
                result.Append('_');
            result.Append(char.ToLowerInvariant(character));
        }
        return result.ToString();
    }
}
