using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace Reverie.Content.Sylvanwalde.Tiles.DruidsGarden
{
    public class LoamGrassSeeds : ModItem
    {
        public override string Texture => Assets.Sylvanwalde.Tiles.DruidsGarden + Name;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<LoamGrassTile>());
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0);
        }
    }
    public class LoamGrassTile : ModTile
    {
        public override string Texture => Assets.Sylvanwalde.Tiles.DruidsGarden + Name;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][ModContent.TileType<LoamTile>()] = true;
            TileID.Sets.NeedsGrassFramingDirt[ModContent.TileType<LoamTile>()] = Type;
            Main.tileBlockLight[Type] = true;
            DustType = DustID.JungleGrass;

            MineResist = 1.05f;
            HitSound = SoundID.Dig;

            AddMapEntry(new Color(98, 134, 13));
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 5;
    }
}
