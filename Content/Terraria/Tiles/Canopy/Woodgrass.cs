using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Tiles.Canopy
{
    public class Woodgrass : ModTile
    {
        public override string Texture => Assets.Terraria.Tiles.Canopy + Name;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMerge[TileID.LivingWood][Type] = true;
            Main.tileMerge[Type][TileID.LivingWood] = true;
            Main.tileMerge[Type][Type] = true;
            TileID.Sets.NeedsGrassFramingDirt[TileID.LivingWood] = Type;
            TileID.Sets.NeedsGrassFramingDirt[TileID.Dirt] = Type;
            //Main.tileMergeDirt[Type] = true;

            MineResist = 1f;
            DustType = DustID.t_LivingWood;
            RegisterItemDrop(ItemID.Wood);
            AddMapEntry(new Color(100, 150, 8));
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            Tile tileAbove = Framing.GetTileSafely(i, j - 1);
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);

            if (WorldGen.genRand.NextBool() && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                WorldGen.PlaceTile(i, j - 1, (ushort)ModContent.TileType<CanopyFoliage>(), mute: true);
                tileAbove.TileFrameY = 0;
                tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);
                WorldGen.SquareTileFrame(i, j + 1, true);
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, i, j - 1, 1, TileChangeType.None);
                }
            }
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail && Main.rand.NextBool(30))
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<WoodgrassSeeds>());
        }
    }
}