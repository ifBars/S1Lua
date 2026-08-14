namespace S1Lua.Model;

public enum ShopSelectionKind
{
    None,
    Compatible,
    Named
}

public sealed record ShopSelection(ShopSelectionKind Kind, IReadOnlyList<string> Names)
{
    public static ShopSelection None { get; } = new(ShopSelectionKind.None, Array.Empty<string>());
    public static ShopSelection Compatible { get; } = new(ShopSelectionKind.Compatible, Array.Empty<string>());
}

public sealed record ClothingDeclaration(
    string? Slot,
    string? Application,
    string? Asset,
    string? Texture,
    bool? Colorable,
    string? DefaultColor,
    IReadOnlyList<string> BlockedSlots);

public sealed record ItemDeclaration(
    string ModId,
    string LocalId,
    string Id,
    string SourceDirectory,
    string? CloneSourceId,
    string? Name,
    string? Description,
    string? Category,
    int? StackLimit,
    double? Price,
    double? ResellMultiplier,
    bool? Legal,
    string? Icon,
    ClothingDeclaration? Clothing,
    ShopSelection Shops);
