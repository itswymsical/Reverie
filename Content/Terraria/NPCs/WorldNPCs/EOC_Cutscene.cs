using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.NPCs.WorldNPCs
{
    public class EOC_Cutscene : ModNPC
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.EyeofCthulhu}";
        public override void SetDefaults()
        {
           NPC.aiStyle = -1;
           AnimationType = NPCID.EyeofCthulhu;
           NPC.width = 110;
           NPC.height = 166;
           NPC.immortal = true;
           NPC.lifeMax = 100;
        }
    }
}
