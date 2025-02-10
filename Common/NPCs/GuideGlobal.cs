using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Reverie.Common.Systems;
using Reverie.Core.NPCs.Components;
using Reverie.Core.Dialogue;

namespace Reverie.Common.NPCs;

public class GuideGlobalNPC : GlobalNPC, IWorldNPCChat
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


    public void ModifyVanillaChatButtons(NPC npc, ref string button, ref string button2)
    {
        switch (npc.ai[3])
        {
            case 1f:
                button = "Stay";
                button2 = "Wander";
                break;
            case 2f:
                button = "Follow";
                button2 = "Wander";
                break;
            case 3f:
            default:
                button = "Follow";
                button2 = "Stay";
                break;
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
                    DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.GuideCommands_Stay, true);
                    break;
                case 2f: // If staying
                case 3f: // If wandering
                default:
                    worldComponent.Follow(npc, Main.myPlayer);
                    DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.GuideCommands_Follow, true);
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
                    DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.GuideCommands_Wander, true);
                    break;
                case 3f: // If wandering
                    worldComponent.Stay(npc);
                    DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.GuideCommands_Stay, true);
                    break;
            }
        }
    }
}