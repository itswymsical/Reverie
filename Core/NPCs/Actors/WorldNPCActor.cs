using Terraria.UI.Chat;

namespace Reverie.Core.NPCs.Actors;

public abstract class WorldNPCActor : ModNPC
{
    public override void SetStaticDefaults()
    {
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
    }
    public override void SetDefaults()
    {
        NPC.aiStyle = 7;
        NPC.homeless = true;
        NPC.immortal = true;
        NPC.life = 300;
        NPC.defense = 10;
        NPC.damage = 15;
        NPC.friendly = true;
    }
    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = "Chat";
        button2 = "Tasks";
    }
}