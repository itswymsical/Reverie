using Terraria;
using Terraria.ID;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Common.Players;

namespace Reverie.Content.Terraria.NPCs.WorldNPCs
{
    public class Argie : MissionNPC
    {
        public override string Texture => Assets.Terraria.NPCs.WorldNPCs + Name;
        public override string HeadTexture => Assets.Terraria.NPCs.WorldNPCs + Name + "_Head";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
            NPCID.Sets.HasNoPartyText[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1.2f,
                Direction = 1
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 38;
            NPC.height = 32;

            NPC.damage = 0;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.immortal = true;
            NPC.GivenName = "Argie";
            NPC.aiStyle = -1;
        }
        public override void FindFrame(int frameHeight)
        {
            if (++NPC.frameCounter >= 4)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (Main.npcFrameCount[Type] * frameHeight);
            }
        }
        public override bool CanTownNPCSpawn(int numTownNPCs) => false;

        public override string GetChat()
        {
            MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
 
            return base.GetChat();
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            base.SetChatButtons(ref button, ref button2);
            button = "Talk";
            button2 = "Pet";
        }
    }
}