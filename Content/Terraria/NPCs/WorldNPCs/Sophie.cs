using Terraria;
using Terraria.ID;
using Reverie.Core.Missions;

namespace Reverie.Content.Terraria.NPCs.WorldNPCs
{
    public class Sophie : MissionNPC
    {
        public override string Texture => Assets.Terraria.NPCs.WorldNPCs + Name;
        public override string HeadTexture => Assets.Terraria.NPCs.WorldNPCs + Name + "_Head";
        public override void SetStaticDefaults()
        {
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
            NPC.width = 38;
            NPC.height = 50;

            NPC.damage = 0;
            NPC.defense = 15;
            NPC.lifeMax = 250;

            NPC.GivenName = "Sophie";
        }
        public override bool CanTownNPCSpawn(int numTownNPCs) => false;       
    }
}