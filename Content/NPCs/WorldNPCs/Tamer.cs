
using Reverie.Core.Dialogue;
using Reverie.Core.NPCs.Actors;

namespace Reverie.Content.NPCs.WorldNPCs;

[AutoloadHead]
public class Tamer : WorldNPCActor
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.npcFrameCount[Type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = 7; // Town NPC AI style
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;

        AnimationType = NPCID.Guide; // Uses Guide's animation
    }

    public override string GetChat()
    {
        return Main.rand.Next(4) switch
        {
            0 => "I can teach you the ways of taming creatures.",
            1 => "Looking for a companion on your journey? I might be able to help.",
            2 => "The wild creatures of this world can be befriended with the right approach.",
            _ => "Taming isn't just about control - it's about mutual respect.",
        };
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = "Taming Shop";

        button2 = "Missions";
    }
    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (!firstButton)
        {
            DialogueManager.Instance.StartDialogueByKey(
                    NPCManager.Default,
                    DialogueKeys.TamerMissions.Chapter1,
                    lineCount: 2,
                    zoomIn: true);
        }
    }
}