using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Tiles.Canopy
{
    public class Brambleroot : ModTile
    {
        public override string Texture => Assets.Terraria.Tiles.Canopy + Name;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            DustType = DustID.t_LivingWood;
            MineResist = 1.2f;
            HitSound = SoundID.Dig;

            AddMapEntry(new Color(162, 124, 92));
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 4;
        }
    }
}