using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace Reverie.Content.Sylvanwalde.Tiles
{
    public class Loam : ModItem
    {
        public override string Texture => Assets.Sylvanwalde.Tiles.Path + Name;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<LoamTile>());
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0);
        }
    }

    public class LoamTile : ModTile
    {
        public override string Texture => Assets.Sylvanwalde.Tiles.Path + Name;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][ModContent.TileType<LoamGrassTile>()] = true;
            Main.tileMerge[TileID.Mud][Type] = true;
            Main.tileBlockLight[Type] = true;
            DustType = DustID.Mud;
            MineResist = 1.05f;

            HitSound = SoundID.Dig;

            AddMapEntry(new Color(132, 114, 97));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 5;
    }
}
