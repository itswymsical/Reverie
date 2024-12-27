using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;

namespace Reverie.Core.Missions
{
    public abstract class MissionNPC : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;

            NPC.aiStyle = 7;

            NPC.immortal = true;
            NPC.knockBackResist = 0.5f;

            AnimationType = -1;
            TownNPCStayingHomeless = true;
        }
        public override void AI()
        {
            base.AI();
            NPC.homeless = true;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "View Missions";
            button2 = "Chat";
        }
        public override string GetChat() => "Hello!";       
    }
}