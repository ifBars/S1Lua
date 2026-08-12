using S1API.Entities;
using S1API.Map;
using S1API.Utils;
using S1Lua.Hosting;
using UnityEngine;

namespace S1Lua.Runtime;

internal sealed class S1ApiMapMarkerService
{
    internal IDisposable Create(MapMarkerRequest request)
    {
        var builder = new MapPOIBuilder(request.Id)
            .WithLabel(request.Label)
            .WithTextVisibility(ParseVisibility(request.TextVisibility))
            .WithVisibility(request.Visible);

        if (request.NpcId != null)
        {
            NPC? npc = NPC.Get(request.NpcId);
            if (npc == null)
                throw new InvalidOperationException($"NPC '{request.NpcId}' was not found.");
            builder.WithTarget(npc.Transform);
        }
        else
        {
            builder.WithPosition(new Vector3(
                (float)request.X!.Value,
                (float)request.Y!.Value,
                (float)request.Z!.Value));
        }

        if (request.Icon != null)
        {
            string path = ModAssetPaths.ResolvePng(request.SourceDirectory, request.Icon, "marker icon");
            Sprite? sprite = ImageUtils.LoadImage(path);
            if (sprite == null)
                throw new InvalidOperationException($"Could not load marker icon '{request.Icon}'.");
            builder.WithIcon(sprite);
        }

        return builder.Build();
    }

    private static MapPOITextVisibility ParseVisibility(string value) => value switch
    {
        "always" => MapPOITextVisibility.Always,
        "hover" => MapPOITextVisibility.OnHover,
        "off" => MapPOITextVisibility.Off,
        _ => throw new InvalidOperationException($"Unknown marker text visibility '{value}'.")
    };
}
