using Reverie.Common.Players;
using Reverie.Core.Missions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;

namespace Reverie.Content.Missions.NPCs
{
    public class MerchantsBrother : MissionNPC
    {
        public override string Texture => Assets.Terraria.NPCs.WorldNPCs + "CowboyApprentice";
        public override string HeadTexture => Assets.Terraria.NPCs.WorldNPCs + "CowboyApprentice" + "_Head";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
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
            NPC.GivenName = "Merhcant's Brother";
            NPC.aiStyle = -1;
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
            button2 = "Missions";
        }
    }
}
