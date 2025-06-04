using Terraria.DataStructures;
using Reverie.Common.Players;

namespace Reverie.Content.Buffs;

public class BarkskinPotionBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.pvpBuff[Type] = true; 
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense += 3;
        player.endurance += 0.05f;
        player.moveSpeed -= 0.09f;

        //var tPlayer = ModContent.GetInstance<ReveriePlayer>();

        //tPlayer.barkskinEnabled = true;
    }
}
