using System;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;
// Pulled from source code
public class LivingTreePass : GenPass
{
    private readonly int _centerAvoidanceDistance = 200;

    public LivingTreePass() : base("Garuanteed Living Trees 1/2", 50f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing living trees";

        var worldSizeFactor = Main.maxTilesX / 4200.0;
        var treeCount = WorldGen.genRand.Next(0, (int)(2.0 * worldSizeFactor) + 1);

        if (WorldGen.drunkWorldGen)
            treeCount += (int)(2.0 * worldSizeFactor);
        else if (Main.tenthAnniversaryWorld)
            treeCount += (int)(3.0 * worldSizeFactor);
        else if (WorldGen.remixWorldGen)
            treeCount += (int)(2.0 * worldSizeFactor);

        if (treeCount == 0 && WorldGen.genRand.NextBool(2))
            treeCount++;

        for (var treeIndex = 0; treeIndex < treeCount; treeIndex++)
        {
            var treeSuccessfullyPlaced = false;
            var attempts = 0;

            while (!treeSuccessfullyPlaced)
            {
                attempts++;
                if (attempts > Main.maxTilesX / 2)
                {
                    treeSuccessfullyPlaced = true;
                    continue;
                }

                var treeX = WorldGen.genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);

                // Adjust position for tenth anniversary world
                if (WorldGen.tenthAnniversaryWorldGen && !WorldGen.remixWorldGen)
                    treeX = WorldGen.genRand.Next((int)(Main.maxTilesX * 0.15), (int)(Main.maxTilesX * 0.85f));

                // Skip if too close to center
                if (treeX <= Main.maxTilesX / 2 - _centerAvoidanceDistance || treeX >= Main.maxTilesX / 2 + _centerAvoidanceDistance)
                {
                    int treeY;
                    for (treeY = 0; !Main.tile[treeX, treeY].HasTile && treeY < Main.worldSurface; treeY++) { }

                    // Check if suitable ground (dirt)
                    if (Main.tile[treeX, treeY].TileType == 0)
                    {
                        treeY--; // Move up one to plant on top of dirt

                        if (treeY > 150) // Not too high up
                        {
                            // Check if area is clear of certain tile types
                            var areaClear = CheckAreaIsSuitable(treeX, treeY);

                            // Also check if not near existing micro caves
                            for (var caveIndex = 0; caveIndex < GenVars.numMCaves; caveIndex++)
                            {
                                if (treeX > GenVars.mCaveX[caveIndex] - 50 && treeX < GenVars.mCaveX[caveIndex] + 50)
                                {
                                    areaClear = false;
                                    break;
                                }
                            }

                            // If area is suitable, grow the main tree
                            if (areaClear)
                            {
                                treeSuccessfullyPlaced = WorldGen.GrowLivingTree(treeX, treeY);

                                // If main tree placed successfully, add smaller trees nearby
                                if (treeSuccessfullyPlaced)
                                {
                                    AddSmallerTreesNearby(treeX, treeY);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Make sure the living leaf block is not solid (property set)
        Main.tileSolid[192] = false;
    }

    private bool CheckAreaIsSuitable(int centerX, int centerY)
    {
        for (var x = centerX - 50; x < centerX + 50; x++)
        {
            for (var y = centerY - 50; y < centerY + 50; y++)
            {
                if (Main.tile[x, y].HasTile)
                {
                    // Check for incompatible tile types
                    switch (Main.tile[x, y].TileType)
                    {
                        case 41:  // Demon Altar
                        case 43:  // Shadow Orb
                        case 44:  // Meteorite
                        case 189: // Corrupt Plant
                        case 196: // Corrupt Bush
                        case 460: // Crimson Plant
                        case 481: // Plants
                        case 482: // Plants
                        case 483: // Plants
                            return false;
                    }
                }
            }
        }
        return true;
    }

    private void AddSmallerTreesNearby(int mainTreeX, int mainTreeY)
    {
        for (var direction = -1; direction <= 1; direction += 2)
        {
            var currentX = mainTreeX;
            var numberOfTrees = WorldGen.genRand.Next(4);

            if (WorldGen.drunkWorldGen || Main.tenthAnniversaryWorld)
                numberOfTrees += WorldGen.genRand.Next(2, 5);
            else if (WorldGen.remixWorldGen)
                numberOfTrees += WorldGen.genRand.Next(1, 6);

            for (var treeIndex = 0; treeIndex < numberOfTrees; treeIndex++)
            {
                currentX += WorldGen.genRand.Next(13, 31) * direction;

                if (currentX <= Main.maxTilesX / 2 - _centerAvoidanceDistance || currentX >= Main.maxTilesX / 2 + _centerAvoidanceDistance)
                {
                    var groundY = mainTreeY;
                    if (Main.tile[currentX, groundY].HasTile)
                    {
                        while (Main.tile[currentX, groundY].HasTile)
                        {
                            groundY--;
                        }
                    }
                    else
                    {
                        while (!Main.tile[currentX, groundY].HasTile)
                        {
                            groundY++;
                        }
                        groundY--;
                    }

                    var areaClear = CheckAreaIsSuitable(currentX, groundY);

                    if (areaClear)
                    {
                        WorldGen.GrowLivingTree(currentX, groundY, patch: true);
                    }
                }
            }
        }
    }
}

public class LivingTreeWallPass : GenPass
{
    public LivingTreeWallPass() : base("Garuanteed Living Tree 2/2", 10f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Adding tree wall details";

        for (var x = 25; x < Main.maxTilesX - 25; x++)
        {
            for (var y = 25; y < Main.worldSurface; y++)
            {
                if (Main.tile[x, y].TileType == 191 ||
                    Main.tile[x, y - 1].TileType == 191 ||
                    Main.tile[x - 1, y].TileType == 191 ||
                    Main.tile[x + 1, y].TileType == 191 ||
                    Main.tile[x, y + 1].TileType == 191)
                {
                    var allMatchingTiles = true;
                    for (var checkX = x - 1; checkX <= x + 1; checkX++)
                    {
                        for (var checkY = y - 1; checkY <= y + 1; checkY++)
                        {
                            if (checkX != x && checkY != y &&
                                Main.tile[checkX, checkY].TileType != 191 &&
                                Main.tile[checkX, checkY].WallType != 244)
                            {
                                allMatchingTiles = false;
                            }
                        }
                    }

                    if (allMatchingTiles)
                    {
                        Main.tile[x, y].WallType = 244;
                    }
                }
            }
        }
    }
}