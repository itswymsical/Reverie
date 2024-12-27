using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Reverie.Content.Terraria.Tiles.Canopy
{
    public class CanopyFoliage : ModTile
    {
        public override string Texture => Assets.Terraria.Tiles.Canopy + Name;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileSolid[Type] = false;

            TileID.Sets.SwaysInWindBasic[Type] = true;
            TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

            TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<Woodgrass>()];
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.WaterDeath = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = [18];

            TileObjectData.addTile(Type);

            DustType = DustID.JunglePlants;
            HitSound = SoundID.Grass;
            AddMapEntry(new Color(100, 190, 8));
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 3;
        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
        {
            if (i % 2 == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
        }
    }
}
