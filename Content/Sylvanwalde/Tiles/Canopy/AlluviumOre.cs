using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Sylvanwalde.Tiles.Canopy
{
    public class AlluviumOreTile : ModTile
    {
        public override string Texture => Assets.Sylvanwalde.Tiles.Canopy + Name;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][TileID.LivingWood] = true;
            Main.tileMerge[TileID.LivingWood][Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileSpelunker[Type] = true;
            DustType = 151;
            MineResist = 1.35f;
            MinPick = 50;
            HitSound = SoundID.Tink;

            AddMapEntry(new Color(108, 187, 86));
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0f;
            g = 0.14f;
            b = 0.12f;
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}