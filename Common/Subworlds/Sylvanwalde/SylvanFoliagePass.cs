using Reverie.Content.Tiles.Sylvanwalde;
using Reverie.Content.Tiles.Sylvanwalde.Canopy;
using Reverie.lib;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Sylvanwalde;

public class SylvanFoliagePass : GenPass
{
    public SylvanFoliagePass() : base("[Sylvan] Foliage", 77f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing Foliage";
        DoGrass();
    }
    private void DoGrass()
    {
        for (var x = 5; x < Main.maxTilesX - 5; x++)
        {
            for (var y = 5; y < Main.maxTilesY - 5; y++)
            {
                var tile = Framing.GetTileSafely(x, y);
                var tileAbove = Framing.GetTileSafely(x, y - 1);
                var tileBelow = Framing.GetTileSafely(x, y + 1);

                if (tile.TileType == (ushort)ModContent.TileType<LoamTile>() && !tileAbove.HasTile)
                {
                    tile.TileType = (ushort)ModContent.TileType<LoamGrassTile>();

                    for (var grassX = x - 1; grassX <= x + 1; grassX++) // Changed < to <= to include x+1
                    {
                        for (var grassY = y - 1; grassY <= y + 1; grassY++) // Changed < to <= to include y+1
                        {
                            // Ensure we're within world bounds
                            if (grassX >= 0 && grassX < Main.maxTilesX && grassY >= 0 && grassY < Main.maxTilesY)
                            {
                                var currentTile = Framing.GetTileSafely(grassX, grassY);
                                var grassAbove = Framing.GetTileSafely(grassX, grassY - 1);
                                if (currentTile.HasTile && currentTile.TileType == (ushort)ModContent.TileType<LoamTile>() && !grassAbove.HasTile)
                                {
                                    currentTile.TileType = (ushort)ModContent.TileType<LoamGrassTile>();
                                }
                            }
                        }
                    }

                    //if (WorldGen.genRand.NextBool(15)) // Adjust probability as needed
                    //{
                    //    int wallSize = WorldGen.genRand.Next(5, 14);
                    //    int wallHeight = 1;

                    //    // Find the center position for the wall patch
                    //    int centerX = x;
                    //    int topY = y - 1; // Start 1 tile above the grass

                    //    for (int wallX = -wallSize / 2; wallX <= wallSize / 2; wallX++)
                    //    {
                    //        // Calculate the height of the wall at this x position
                    //        // Walls are taller in the middle, shorter at the edges
                    //        int xFromCenter = Math.Abs(wallX);
                    //        int adjustedHeight = wallHeight - (int)(xFromCenter * 0.7f);

                    //        // Add some noise to make it more natural
                    //        // If you don't have a Noise class, you can use this simpler approach:
                    //        int noiseHeight = WorldGen.genRand.Next(-1, 2);

                    //        // Or if you have the Noise class:
                    //        // float noise = genNoise.GetPerlin((centerX + wallX) % 1000 * 0.1f, centerX % 1000 * 0.1f);
                    //        // int noiseHeight = (int)(noise * 2);

                    //        adjustedHeight += noiseHeight;
                    //        adjustedHeight = Math.Max(2, adjustedHeight); // Ensure minimum height

                    //        // Place the dirt walls
                    //        for (int wallY = 0; wallY < adjustedHeight; wallY++)
                    //        {
                    //            int placeX = centerX + wallX;
                    //            int placeY = topY - wallY;

                    //            if (WorldGen.InWorld(placeX, placeY))
                    //            {
                    //                // Only place walls in empty wall spaces or replacing dirt walls
                    //                if (Main.tile[placeX, placeY].WallType == 0 ||
                    //                    Main.tile[placeX, placeY].WallType == WallID.Dirt)
                    //                {
                    //                    WorldGen.PlaceWall(placeX, placeY, WallID.DirtUnsafe, true);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }

                if (WorldGen.genRand.NextBool(3) && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
                {
                    // Make sure we're not at the top edge of the world
                    if (y > 1) // Ensure there's room for the tile above
                    {
                        WorldGen.PlaceTile(x, y - 1, (ushort)ModContent.TileType<CanopyFoliageTile>(), mute: true);
                        var newTileAbove = Framing.GetTileSafely(x, y - 1);
                        newTileAbove.TileFrameY = 0;
                        newTileAbove.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);
                        WorldGen.SquareTileFrame(x, y - 1, true);
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendTileSquare(-1, x, y - 1, 1, TileChangeType.None);
                        }
                    }
                }

                if (WorldGen.genRand.NextBool(7))
                {
                    // Check if we're too close to the edge for this feature
                    if (x + 5 < Main.maxTilesX && y + 3 < Main.maxTilesY)
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
                }

                if (WorldGen.genRand.NextBool(5) && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
                {
                    if (Main.tile[x, y].HasTile && Main.tile[x, y].BlockType == BlockType.Solid)
                    {
                        WorldGen.PlaceTile(x, y - 1, TileID.Saplings, mute: true);
                        WorldGen.GrowTree(x, y - 1);
                        break;
                    }
                }
            }
        }
    }
}