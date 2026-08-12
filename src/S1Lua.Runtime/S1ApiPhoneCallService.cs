using S1API.Entities;
using S1API.PhoneCalls;
using S1API.Utils;
using S1Lua.Hosting;
using UnityEngine;

namespace S1Lua.Runtime;

internal sealed class S1ApiPhoneCallService
{
    internal bool Queue(PhoneCallRequest request)
    {
        Sprite? icon = null;
        if (request.Icon != null)
        {
            string path = ModAssetPaths.ResolvePng(request.SourceDirectory, request.Icon, "caller icon");
            icon = ImageUtils.LoadImage(path);
            if (icon == null)
                throw new InvalidOperationException($"Could not load caller icon '{request.Icon}'.");
        }

        LuaPhoneCall call;
        if (request.NpcId != null)
        {
            NPC? npc = NPC.Get(request.NpcId);
            if (npc == null)
                return false;
            call = new LuaPhoneCall(npc, request.Stages);
        }
        else
        {
            call = new LuaPhoneCall(request.Caller, icon, request.Stages);
        }

        CallManager.QueueCall(call);
        return true;
    }

    private sealed class LuaPhoneCall : PhoneCallDefinition
    {
        internal LuaPhoneCall(string caller, Sprite? icon, IReadOnlyList<string> stages)
            : base(caller, icon)
        {
            AddStages(stages);
        }

        internal LuaPhoneCall(NPC caller, IReadOnlyList<string> stages)
            : base(caller)
        {
            AddStages(stages);
        }

        private void AddStages(IReadOnlyList<string> stages)
        {
            foreach (string stage in stages)
                AddStage(stage);
        }
    }
}
