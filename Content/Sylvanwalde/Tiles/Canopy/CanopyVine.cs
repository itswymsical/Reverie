using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Reverie.Utilities;

namespace Reverie.Content.Sylvanwalde.Tiles.Canopy
{
    public class CanopyVine : ModTile
    {
        public override string Texture => Assets.Sylvanwalde.Tiles.Canopy + Name;
        public override void SetStaticDefaults()
        {
            TileID.Sets.VineThreads[Type] = true;
            TileID.Sets.IsVine[Type] = true;
            TileID.Sets.SwaysInWindBasic[Type] = true;

            Main.tileCut[Type] = true;     
            Main.tileLavaDeath[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLighted[Type] = true;
            HitSound = SoundID.Grass;
            DustType = DustID.JunglePlants;

            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
            AddMapEntry(new Color(95, 143, 65));
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.03f;
            g = 0.08f;
            b = 0.12f;
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile tile = Framing.GetTileSafely(i, j + 1);
            if (tile.HasTile && tile.TileType == Type)
            {
                WorldGen.KillTile(i, j + 1);
            }
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tile = Framing.GetTileSafely(i, j - 1);
            int type = -1;
            if (tile.HasTile && !tile.BottomSlope)
                type = tile.TileType;

            if (type == ModContent.TileType<Woodgrass>() || type == TileID.LivingWood || type == Type) {
                return true;
            }

            WorldGen.KillTile(i, j);
            return true;
        }
        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j + 1);
            if (WorldGen.genRand.NextBool(10) && !tile.HasTile && !(tile.LiquidType == LiquidID.Lava))
            {
                bool placed = false;
                int Test = j;
                while (Test > j - 10)
                {
                    Tile testTile = Framing.GetTileSafely(i, Test);
                    if (testTile.BottomSlope)
                    {
                        break;
                    }
                    else if (!testTile.HasTile || testTile.TileType != ModContent.TileType<Woodgrass>())
                    {
                        Test--;
                        continue;
                    }
                    placed = true;
                    break;
                }
                if (placed && CanGrowMoreVines(i, j + 1))  // Add this condition
                {
                    tile.TileType = Type;
                    tile.HasTile = true;
                    WorldGen.SquareTileFrame(i, j + 1, true);
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendTileSquare(-1, i, j + 1, 3, TileChangeType.None);
                    }
                }
            }
        }
        private static bool CanGrowMoreVines(int centerX, int centerY)
        {
            const int WorldBoundary = 30;
            if (!WorldGen.InWorld(centerX, centerY, WorldBoundary))
                return false;

            const int HorizontalSearchRange = 4;
            const int VerticalSearchRangeUp = 6;
            const int VerticalSearchRangeDown = 10;
            const int MaxVineCount = 60;
            int SpecialVineType = ModContent.TileType<CanopyVine>();

            int vineCount = 0;
            int adjustedMaxVineCount = (Main.tile[centerX, centerY].TileType == SpecialVineType)
                ? MaxVineCount / 5
                : MaxVineCount;

            for (int x = centerX - HorizontalSearchRange; x <= centerX + HorizontalSearchRange; x++)
            {
                for (int y = centerY - VerticalSearchRangeUp; y <= centerY + VerticalSearchRangeDown; y++)
                {
                    if (TileID.Sets.IsVine[Main.tile[x, y].TileType])
                    {
                        vineCount++;

                        if (y > centerY && CanDrawLineBetweenPoints(centerX, centerY, x, y))
                        {
                            int verticalDistance = y - centerY;
                            vineCount += (Main.tile[x, y].TileType != SpecialVineType)
                                ? verticalDistance * 2
                                : verticalDistance * 20;
                        }

                        if (vineCount > adjustedMaxVineCount)
                            return false;
                    }
                }
            }
            return true;
        }
        private static bool CanDrawLineBetweenPoints(int x1, int y1, int x2, int y2)
        {
            return Collision.CanHitLine(
                new Vector2(x1 * 16, y1 * 16), 1, 1,
                new Vector2(x2 * 16, y2 * 16), 1, 1
            );
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => true;       
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new(texture.Width * 0.3f, texture.Height * 0.55f);

            Tile tile = Framing.GetTileSafely(i, j);
            Color color = new(5, 143, 65);

            Texture2D glow = ModContent.Request<Texture2D>($"{Assets.Sylvanwalde.Tiles.Canopy}CanopyVine_Glow").Value;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
            Main.EntitySpriteDraw(
                glow, 
                new Vector2(i, j) - Main.screenPosition + zero - new Vector2(0, 0), 
                new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), 
                color * .6f,
                0f,
                origin,
                1f,
                SpriteEffects.None);
        }
    }
}
