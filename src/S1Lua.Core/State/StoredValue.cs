namespace S1Lua.State;

public enum StoredValueKind
{
    String,
    Number,
    Boolean
}

public sealed class StoredValue
{
    public StoredValueKind Kind { get; set; }
    public string? String { get; set; }
    public double Number { get; set; }
    public bool Boolean { get; set; }

    public static StoredValue FromString(string value) => new() { Kind = StoredValueKind.String, String = value };
    public static StoredValue FromNumber(double value) => new() { Kind = StoredValueKind.Number, Number = value };
    public static StoredValue FromBoolean(bool value) => new() { Kind = StoredValueKind.Boolean, Boolean = value };
}
