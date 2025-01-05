using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace Reverie.Content.Sylvanwalde.Tiles
{
    public class Cobblestone : ModItem
    {
        public override string Texture => Assets.Sylvanwalde.Tiles.Path + Name;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CobblestoneTile>());
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0);
        }
    }

    public class CobblestoneTile : ModTile
    {
        public override string Texture => Assets.Sylvanwalde.Tiles.Path + Name;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][ModContent.TileType<LoamTile>()] = true;
            TileID.Sets.NeedsGrassFramingDirt[ModContent.TileType<LoamTile>()] = Type;
            Main.tileMerge[TileID.Mud][Type] = true;
            Main.tileBlockLight[Type] = true;
            DustType = DustID.Stone;
            MineResist = 1.25f;

            HitSound = SoundID.Tink;

            AddMapEntry(new Color(101, 76, 109));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 5;   
    }
}
