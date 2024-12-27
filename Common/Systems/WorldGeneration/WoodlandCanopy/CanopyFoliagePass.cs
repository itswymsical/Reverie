using Reverie.Content.Terraria.Tiles.Canopy;
using Reverie.Content.Tiles.Canopy;
using Reverie.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Reverie.Common.Systems.WorldGeneration.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Common.Systems.WorldGeneration.WoodlandCanopy
{
    public class CanopyFoliagePass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Sprucing up the Woodland Canopy";
            GrowAmbientTiles();
        }
        public void GrowAmbientTiles()
        {
            for (int x = canopyX - canopyRadiusH; x <= canopyX + canopyRadiusH; x++)
            {
                for (int y = canopyY - canopyRadiusV; y <= canopyY + canopyRadiusV; y++)
                {
                    if (Helper.GenerateCanopyShape(x, y, canopyX, canopyY, canopyRadiusH, canopyRadiusV, 0.04f, canopyRadiusH / 4, 100, 15))
                    {
                        Tile tile = Framing.GetTileSafely(x, y);
                        Tile tileAbove = Framing.GetTileSafely(x, y - 1);
                        Tile tileBelow = Framing.GetTileSafely(x, y + 1);

                        if (tile.TileType == TileID.LivingWood && !tileAbove.HasTile)
                        {
                            tile.TileType = (ushort)canopyBlock;

                            for (int grassX = x - 1; grassX <= x + 1; grassX++)
                            {
                                for (int grassY = y - 1; grassY <= y + 1; grassY++)
                                {
                                    Tile currentTile = Framing.GetTileSafely(grassX, grassY);
                                    Tile grassAbove = Framing.GetTileSafely(grassX, grassY - 1);

                                    if (currentTile.HasTile && currentTile.TileType == TileID.LivingWood && !grassAbove.HasTile)
                                    {
                                        currentTile.TileType = (ushort)canopyBlock;
                                    }
                                }
                            }
                        }

                        // GRASS PLANTS
                        if (WorldGen.genRand.NextBool(3) && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
                        {
                            WorldGen.PlaceTile(x, y - 1, (ushort)ModContent.TileType<CanopyFoliage>());
                            tileAbove.TileFrameY = 0;
                            tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);
                            WorldGen.SquareTileFrame(x, y + 1, true);
                            if (Main.netMode == NetmodeID.Server)
                            {
                                NetMessage.SendTileSquare(-1, x, y - 1, 1, TileChangeType.None);
                            }
                        }

                        // VINES
                        if (WorldGen.genRand.NextBool(8) && !tileBelow.HasTile && !tileBelow.LeftSlope && !tileBelow.RightSlope && !tileBelow.IsHalfBlock)
                        {
                            if (!tile.BottomSlope)
                            {
                                tileBelow.TileType = (ushort)ModContent.TileType<CanopyVine>();
                                tileBelow.HasTile = true;
                                WorldGen.SquareTileFrame(x, y + 1, true);
                                if (Main.netMode == NetmodeID.Server)
                                {
                                    NetMessage.SendTileSquare(-1, x, y + 1, 3, 0);
                                }
                            }
                        }

                        // SAPLINGS
                        if (WorldGen.genRand.NextBool(18) && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
                        {
                            if (Main.tile[x, y].HasTile && Main.tile[x, y].BlockType == BlockType.Solid)
                            {
                                WorldGen.PlaceTile(x, y - 1, TileID.Saplings, mute: true);
                                WorldGen.GrowTree(x, y - 1);
                                break;
                            }
                        }

                        // PILES
                        if (WorldGen.genRand.NextBool(7))
                        {
                            bool hasRoom = true;
                            for (int checkX = x; checkX < x + 5; checkX++)
                            {
                                for (int checkY = y; checkY < y + 3; checkY++)
                                {
                                    Tile tile5 = Framing.GetTileSafely(checkX, checkY);
                                    if (tile5.HasTile)
                                    {
                                        hasRoom = false;
                                        break;
                                    }
                                }
                                if (!hasRoom)
                                    break;
                            }

                            if (hasRoom)
                            {
                                int tileType = WorldGen.genRand.NextBool() ? ModContent.TileType<CanopyLogFoliage>() : ModContent.TileType<CanopyRockFoliage>();
                                WorldGen.PlaceTile(x, y, tileType, mute: true);
                                if (Main.netMode == NetmodeID.Server)
                                {
                                    NetMessage.SendTileSquare(-1, x, y, 5, TileChangeType.None);
                                }
                            }
                        }

                        // Grass again to ensure 0 patches

                        if (tile.TileType == TileID.LivingWood && !tileAbove.HasTile)
                        {
                            tile.TileType = (ushort)canopyBlock;

                            for (int grassX = x - 1; grassX <= x + 1; grassX++)
                            {
                                for (int grassY = y - 1; grassY <= y + 1; grassY++)
                                {
                                    Tile currentTile = Framing.GetTileSafely(grassX, grassY);
                                    Tile grassAbove = Framing.GetTileSafely(grassX, grassY - 1);

                                    if (currentTile.HasTile && currentTile.TileType == TileID.LivingWood && !grassAbove.HasTile)
                                    {
                                        currentTile.TileType = (ushort)canopyBlock;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}