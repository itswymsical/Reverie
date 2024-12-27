using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace Reverie.Content.Archaea.Tiles
{
    public class Carbon : ModTile
    {
        public override string Texture => Assets.Archaea.Tiles.Emberite + Name;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][TileID.Sandstone] = true;
            Main.tileMerge[TileID.Sandstone][Type] = true;
            Main.tileBlockLight[Type] = true;
            DustType = DustID.Smoke;
            MineResist = 1.35f;
            MinPick = 50;
            HitSound = SoundID.Tink;

            AddMapEntry(new Color(98, 88, 70));
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 5;
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            base.KillTile(i, j, ref fail, ref effectOnly, ref noItem);
        }
    }
}
