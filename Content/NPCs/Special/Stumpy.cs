using Reverie.Core.NPCs.Actors;

namespace Reverie.Content.NPCs.Special;

public class Stumpy : WorldNPCActor
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
        NPC.immortal = true;
        NPC.width = 50;
        NPC.height = 74;
        NPC.aiStyle = 7;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
    }

    public override string GetChat()
    {
        return Main.rand.Next() switch
        {
            _ => "...",
        };
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = "Chat";

        button2 = "Missions";
    }
}