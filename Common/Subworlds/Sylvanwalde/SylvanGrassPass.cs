using Reverie.Content.Tiles.Sylvanwalde;
using Reverie.lib;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Archaea.Generation;

public class SylvanGrassPass : GenPass
{
    public SylvanGrassPass() : base("[Sylvan] Grass", 77f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing grass";
        DoGrass();
    }
    private void DoGrass()
    {
        for (int x = 0; x <= Main.maxTilesX; x++)
        {
            for (int y = 0; y <= Main.maxTilesY; y++)
            {
                var tile = Framing.GetTileSafely(x, y);
                var tileAbove = Framing.GetTileSafely(x, y - 1);
                var tileBelow = Framing.GetTileSafely(x, y + 1);

                if (tile.TileType == (ushort)ModContent.TileType<LoamTile>() && !tileAbove.HasTile)
                {

                    tile.TileType = (ushort)ModContent.TileType<LoamGrassTile>();

                    for (var grassX = x - 1; grassX < x + 1; grassX++)
                    {
                        for (var grassY = y - 1; grassY < y + 1; grassY++)
                        {
                            var currentTile = Framing.GetTileSafely(grassX, grassY);
                            var grassAbove = Framing.GetTileSafely(grassX, grassY - 1);
                            if (currentTile.HasTile && currentTile.TileType == (ushort)ModContent.TileType<LoamTile>() && !grassAbove.HasTile)
                            {
                                currentTile.TileType = (ushort)ModContent.TileType<LoamGrassTile>();
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
            }
        }
    }
}