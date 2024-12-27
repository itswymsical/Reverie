using Reverie.Core.Missions;
using Terraria;
using Terraria.ID;

namespace Reverie.Content.Terraria.NPCs.WorldNPCs
{
    public class Cheeks : MissionNPC
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.Squirrel}";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Squirrel];
            NPCID.Sets.HasNoPartyText[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 2f,
                Direction = -1
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 50;
            NPC.height = 32;

            NPC.damage = 0;
            NPC.defense = 15;
            NPC.lifeMax = 50;
            NPCID.Sets.TownCritter[Type] = true;
            NPC.GivenName = "Cheeks";
            AIType = NPCID.TownBunny;
            AnimationType = NPCID.Squirrel;
        }
        public override bool CanTownNPCSpawn(int numTownNPCs) => false;
    }
}
