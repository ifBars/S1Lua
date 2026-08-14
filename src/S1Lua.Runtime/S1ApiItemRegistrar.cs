using S1API.Items;
using S1API.Rendering;
using S1API.Shops;
using S1API.Utils;
using S1Lua.Hosting;
using S1Lua.Model;
using UnityEngine;
using ClothingApplicationType = S1API.Items.Clothing.ClothingApplicationType;
using ClothingColor = S1API.Items.Clothing.ClothingColor;
using ClothingDefinition = S1API.Items.Clothing.ClothingItemDefinition;
using ClothingItemCreator = S1API.Items.Clothing.ClothingItemCreator;
using ClothingItemDefinitionBuilder = S1API.Items.Clothing.ClothingItemDefinitionBuilder;
using ClothingSlot = S1API.Items.Clothing.ClothingSlot;
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

        bool clothingRequested = declaration.Clothing != null ||
                                 category == ItemCategory.Clothing ||
                                 source is ClothingDefinition;
        if (clothingRequested)
            return RegisterClothing(declaration, source, name, description);

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

    private StorableItemDefinition RegisterClothing(
        ItemDeclaration declaration,
        ItemDefinition? source,
        string name,
        string description)
    {
        ClothingDefinition? clothingSource = source as ClothingDefinition;
        if (source != null && clothingSource == null)
        {
            throw new InvalidOperationException(
                $"The clone source '{declaration.CloneSourceId}' is not a clothing item.");
        }

        ClothingItemDefinitionBuilder builder = clothingSource == null
            ? ClothingItemCreator.CreateBuilder()
            : ClothingItemCreator.CloneFrom(clothingSource) ??
              throw new InvalidOperationException($"Could not clone clothing item '{declaration.CloneSourceId}'.");

        builder.WithBasicInfo(declaration.Id, name, description, ItemCategory.Clothing);
        if (declaration.StackLimit.HasValue)
            builder.WithStackLimit(declaration.StackLimit.Value);
        if (declaration.Price.HasValue || declaration.ResellMultiplier.HasValue)
        {
            float price = (float)(declaration.Price ?? clothingSource?.BasePurchasePrice ?? 10d);
            float resell = (float)(declaration.ResellMultiplier ?? clothingSource?.ResellMultiplier ?? 0.5d);
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

        ApplyClothingOptions(builder, declaration, clothingSource, name);

        ClothingDefinition created = builder.Build();
        _host.Log(S1LuaLogLevel.Info, declaration.ModId, $"Registered clothing item '{declaration.Id}'.");
        return created;
    }

    private static void ApplyClothingOptions(
        ClothingItemDefinitionBuilder builder,
        ItemDeclaration declaration,
        ClothingDefinition? source,
        string name)
    {
        ClothingDeclaration? options = declaration.Clothing;
        if (options == null)
        {
            if (source == null)
                throw new InvalidOperationException("Clothing items must clone clothing or provide clothing options.");
            return;
        }

        ClothingSlot? slot = options.Slot == null
            ? null
            : ParseClothingEnum<ClothingSlot>(options.Slot, "slot");
        ClothingApplicationType application = options.Application == null
            ? source?.ApplicationType ?? ClothingApplicationType.Accessory
            : ParseClothingEnum<ClothingApplicationType>(options.Application, "application");

        if (slot.HasValue)
            builder.WithSlot(slot.Value);
        if (options.Application != null)
            builder.WithApplicationType(application);
        if (options.Colorable.HasValue)
            builder.WithColorable(options.Colorable.Value);
        if (options.DefaultColor != null)
            builder.WithDefaultColor(ParseClothingEnum<ClothingColor>(options.DefaultColor, "default_color"));
        if (options.BlockedSlots.Count > 0)
        {
            ClothingSlot[] blockedSlots = options.BlockedSlots
                .Select(value => ParseClothingEnum<ClothingSlot>(value, "blocked_slots"))
                .ToArray();
            builder.WithBlockedSlots(blockedSlots);
        }

        string? sourceAsset = options.Asset ?? source?.ClothingAssetPath;
        if (options.Texture == null)
        {
            if (sourceAsset == null)
                throw new InvalidOperationException("Clothing must provide an asset path or clone an existing clothing item.");
            if (options.Asset != null)
                builder.WithClothingAsset(options.Asset);
            return;
        }
        if (sourceAsset == null)
            throw new InvalidOperationException("Clothing with a custom texture needs an asset path to clone.");

        string texturePath = ModAssetPaths.ResolvePng(
            declaration.SourceDirectory,
            options.Texture,
            "clothing texture");
        Texture2D? texture = TextureUtils.LoadTextureFromFile(texturePath);
        if (texture == null)
            throw new InvalidOperationException($"Could not load clothing texture '{options.Texture}'.");

        string targetAsset = BuildClothingResourcePath(declaration);
        bool registered = application == ClothingApplicationType.Accessory
            ? AccessoryFactory.CreateAndRegisterAccessory(
                sourceAsset,
                targetAsset,
                name,
                CommonTextureReplacements(texture),
                null)
            : AvatarLayerFactory.CreateAndRegisterAvatarLayer(
                sourceAsset,
                targetAsset,
                name,
                texture);
        if (!registered)
            throw new InvalidOperationException($"Could not create clothing visual from asset '{sourceAsset}'.");

        builder.WithClothingAsset(targetAsset);
    }

    private static Dictionary<string, Texture2D> CommonTextureReplacements(Texture2D texture) =>
        new(StringComparer.Ordinal)
        {
            ["_MainTex"] = texture,
            ["_BaseMap"] = texture,
            ["_BaseColorMap"] = texture
        };

    private static string BuildClothingResourcePath(ItemDeclaration declaration) =>
        $"S1Lua/{ResourceSegment(declaration.ModId)}/{ResourceSegment(declaration.LocalId)}";

    private static string ResourceSegment(string value) =>
        new(value.Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray());

    private static TEnum ParseClothingEnum<TEnum>(string value, string field)
        where TEnum : struct, Enum
    {
        string normalized = value.Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
        foreach (string name in Enum.GetNames(typeof(TEnum)))
        {
            if (string.Equals(name, normalized, StringComparison.OrdinalIgnoreCase))
                return Enum.Parse<TEnum>(name);
        }

        string choices = string.Join(", ", Enum.GetNames(typeof(TEnum)).Select(RuntimeIdentifier.FromPascalCase));
        throw new InvalidOperationException($"Unknown clothing {field} '{value}'. Choose one of: {choices}.");
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
