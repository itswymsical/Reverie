using Reverie.Common.NPCs;

namespace Reverie.Content.NPCs.WorldNPCs;

public class BasicNPC : WorldNPC
{
    public override Color SkinColor => Color.BurlyWood;
    public override int HairType => 55;
    public override Color HairColor => new Color(183, 21, 44);
    public override int ArmorType => 9;
    public override bool WearsHelmet => false;
    public override bool WearsChestplate => true;
    public override bool WearsLeggings => true;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.width = 32;
        NPC.height = 46;
        NPC.aiStyle = 7;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
    }

    public override string GetChat()
    {
        return Main.rand.Next(4) switch
        {
            0 => "bogus.",
            1 => "bongus.",
            2 => "chongus.",
            _ => "boobus chongus.",
        };
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = "Empty button that don't do shit";
    }
}