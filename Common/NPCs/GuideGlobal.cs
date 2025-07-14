using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Reverie.Common.Systems;
using Reverie.Core.NPCs.Components;
using Reverie.Core.Dialogue;

namespace Reverie.Common.NPCs;

public class GuideGlobalNPC : GlobalNPC
{
    private WorldNPCComponent worldComponent;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.Guide;
    }

    public override void SetDefaults(NPC npc)
    {
        if (npc.type == NPCID.Guide)
        {
            if (!npc.TryGetGlobalNPC(out worldComponent))
            {
                worldComponent = new WorldNPCComponent
                {
                    Enabled = true,
                    FollowDistance = 100f
                };
            }
        }
    }

    public override void OnChatButtonClicked(NPC npc, bool firstButton)
    {
        if (worldComponent == null) return;

        if (firstButton)
        {
            switch (npc.ai[3])
            {
                case 1f: // If following
                    worldComponent.Stay(npc);
                    Main.NewText($"{npc.GivenName}: Staying here.");
                    break;
                case 2f: // If staying
                case 3f: // If wandering
                default:
                    worldComponent.Follow(npc, Main.myPlayer);
                    Main.NewText($"{npc.GivenName}: Following you!");
                    break;
            }
        }
        else
        {
            switch (npc.ai[3])
            {
                case 1f: // If following
                case 2f: // If staying
                    worldComponent.Wander(npc);
                    Main.NewText($"{npc.GivenName}: I'ma do my own thing.");
                    break;
                case 3f: // If wandering
                    worldComponent.Stay(npc);
                    Main.NewText($"{npc.GivenName}: Make up your mind, lol. I'll be staying here.");
                    break;
            }
        }
    }
}