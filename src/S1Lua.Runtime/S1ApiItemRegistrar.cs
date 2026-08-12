using S1API.Items;
using S1API.Shops;
using S1API.Utils;
using S1Lua.Hosting;
using S1Lua.Model;
using ItemCreator = S1API.Items.Storable.ItemCreator;
using StorableItemDefinition = S1API.Items.Storable.StorableItemDefinition;

namespace S1Lua.Runtime;

internal sealed class S1ApiItemRegistrar
{
    private readonly IS1LuaHost _host;

    internal S1ApiItemRegistrar(IS1LuaHost host)
    {
        _host = host;
    }

    internal ItemDefinition Register(ItemDeclaration declaration)
    {
        ItemDefinition? existing = ItemManager.GetDefinition(declaration.Id);
        if (existing != null)
            return existing;

        ItemDefinition? source = declaration.CloneSourceId == null
            ? null
            : ItemManager.GetDefinition(declaration.CloneSourceId);
        if (declaration.CloneSourceId != null && source == null)
            throw new InvalidOperationException($"The clone source '{declaration.CloneSourceId}' was not found.");

        string name = declaration.Name ?? source?.Name ?? declaration.LocalId;
        string description = declaration.Description ?? source?.Description ?? string.Empty;
        ItemCategory category = declaration.Category == null
            ? source?.Category ?? ItemCategory.Tools
            : ParseCategory(declaration.Category);

        var builder = declaration.CloneSourceId == null
            ? ItemCreator.CreateBuilder()
            : ItemCreator.CloneFrom(declaration.CloneSourceId);

        builder.WithBasicInfo(declaration.Id, name, description, category);
        if (declaration.StackLimit.HasValue)
            builder.WithStackLimit(declaration.StackLimit.Value);
        if (declaration.Price.HasValue || declaration.ResellMultiplier.HasValue)
        {
            var storableSource = source as StorableItemDefinition;
            float price = (float)(declaration.Price ?? storableSource?.BasePurchasePrice ?? 10d);
            float resell = (float)(declaration.ResellMultiplier ?? storableSource?.ResellMultiplier ?? 0.5d);
            builder.WithPricing(price, resell);
        }
        if (declaration.Legal.HasValue)
            builder.WithLegalStatus(declaration.Legal.Value ? LegalStatus.Legal : LegalStatus.Illegal);
        if (declaration.Icon != null)
        {
            string path = ResolveIconPath(declaration);
            var sprite = ImageUtils.LoadImage(path);
            if (sprite == null)
                throw new InvalidOperationException($"Could not load icon '{declaration.Icon}'.");
            builder.WithIcon(sprite);
        }

        StorableItemDefinition created = builder.Build();
        _host.Log(S1LuaLogLevel.Info, declaration.ModId, $"Registered item '{declaration.Id}'.");
        return created;
    }

    internal void AddToShops(ItemDeclaration declaration, ItemDefinition item)
    {
        int added = declaration.Shops.Kind switch
        {
            ShopSelectionKind.None => 0,
            ShopSelectionKind.Compatible => ShopManager.AddToCompatibleShops(item),
            ShopSelectionKind.Named => ShopManager.AddToShops(item, declaration.Shops.Names.ToArray()),
            _ => 0
        };

        if (declaration.Shops.Kind != ShopSelectionKind.None)
            _host.Log(S1LuaLogLevel.Info, declaration.ModId, $"Added '{declaration.Id}' to {added} shop(s).");
    }

    private static ItemCategory ParseCategory(string value)
    {
        if (Enum.TryParse(value, ignoreCase: true, out ItemCategory category))
            return category;
        string names = string.Join(", ", Enum.GetNames(typeof(ItemCategory)).Select(name => name.ToLowerInvariant()));
        throw new InvalidOperationException($"Unknown category '{value}'. Choose one of: {names}.");
    }

    private static string ResolveIconPath(ItemDeclaration declaration)
    {
        return ModAssetPaths.ResolvePng(declaration.SourceDirectory, declaration.Icon!, "item icon");
    }
}
