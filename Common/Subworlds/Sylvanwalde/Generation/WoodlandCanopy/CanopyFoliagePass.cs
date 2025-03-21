using Reverie.Content.Tiles.Sylvanwalde.Canopy;
using Reverie.Utilities;
using Terraria.IO;
using Terraria.WorldBuilding;

using static Reverie.Common.Subworlds.Sylvanwalde.Generation.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Common.Subworlds.Sylvanwalde.Generation.WoodlandCanopy;

public class CanopyFoliagePass: GenPass
{
    public CanopyFoliagePass() : base("[Sylvan] Canopy Foliage", 66.53f)
    {
    }
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Sprucing up the Woodland Canopy";
        GrowAmbientTiles();
    }

    private void GrowAmbientTiles()
    {
        for (int x = canopyX - canopyRadiusH; x <= canopyX + canopyRadiusH; x++)
        {
            for (int y = canopyY - canopyRadiusV; y <= canopyY + canopyRadiusV; y++)
            {
                if (WorldGenUtils.GenerateCanopyShape(x, y, canopyX, canopyY, canopyRadiusH, canopyRadiusV, 0.04f, canopyRadiusH / 4, 100, 15))
                {
                    var tile = Framing.GetTileSafely(x, y);
                    var tileAbove = Framing.GetTileSafely(x, y - 1);
                    var tileBelow = Framing.GetTileSafely(x, y + 1);

                    if (tile.TileType == TileID.LivingWood && !tileAbove.HasTile)
                    {
                        tile.TileType = (ushort)canopyBlock;

                        for (var grassX = x - 1; grassX <= x + 1; grassX++)
                        {
                            for (var grassY = y - 1; grassY <= y + 1; grassY++)
                            {
                                var currentTile = Framing.GetTileSafely(grassX, grassY);
                                var grassAbove = Framing.GetTileSafely(grassX, grassY - 1);

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
                        WorldGen.PlaceTile(x, y - 1, (ushort)ModContent.TileType<CanopyFoliageTile>());
                        tileAbove.TileFrameY = 0;
                        tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);
                        WorldGen.SquareTileFrame(x, y + 1, true);
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendTileSquare(-1, x, y - 1, 1, TileChangeType.None);
                        }
                    }

                    // VINES
                    //if (WorldGen.genRand.NextBool(8) && !tileBelow.HasTile && !tileBelow.LeftSlope && !tileBelow.RightSlope && !tileBelow.IsHalfBlock)
                    //{
                    //    if (!tile.BottomSlope)
                    //    {
                    //        tileBelow.TileType = (ushort)ModContent.TileType<CanopyVine>();
                    //        tileBelow.HasTile = true;
                    //        WorldGen.SquareTileFrame(x, y + 1, true);
                    //        if (Main.netMode == NetmodeID.Server)
                    //        {
                    //            NetMessage.SendTileSquare(-1, x, y + 1, 3, 0);
                    //        }
                    //    }
                    //}

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
                        var hasRoom = true;
                        for (var checkX = x; checkX < x + 5; checkX++)
                        {
                            for (var checkY = y; checkY < y + 3; checkY++)
                            {
                                var tile5 = Framing.GetTileSafely(checkX, checkY);
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
                            var tileType = WorldGen.genRand.NextBool() ? ModContent.TileType<CanopyLogTile>() : ModContent.TileType<CanopyLogTile>();
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

                        for (var grassX = x - 1; grassX <= x + 1; grassX++)
                        {
                            for (var grassY = y - 1; grassY <= y + 1; grassY++)
                            {
                                var currentTile = Framing.GetTileSafely(grassX, grassY);
                                var grassAbove = Framing.GetTileSafely(grassX, grassY - 1);

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