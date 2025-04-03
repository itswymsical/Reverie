using Reverie.Content.Tiles.Taiga;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TundraPass : GenPass
{
    public TundraPass() : base("[Reverie] Tundra Biome", 247.43f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating Tundra Base";
        GenerateBaseTundra(progress);

        progress.Message = "Adding Taiga Surface";
        ApplyTaigaSurface();
        BlendBiomes();

        progress.Message = "Adding Taiga Details";
        AddTaigaDetails();

        progress.Set(1.0f);
    }

    private void GenerateBaseTundra(GenerationProgress progress)
    {
        GenVars.snowTop = (int)Main.worldSurface;
        int snowDepthBound = GenVars.lavaLine - WorldGen.genRand.Next(160, 200);
        int lavaLineDepth = GenVars.lavaLine;

        if (WorldGen.remixWorldGen)
        {
            lavaLineDepth = Main.maxTilesY - 250;
            snowDepthBound = lavaLineDepth - WorldGen.genRand.Next(160, 200);
        }

        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;
        int columnDepth = 10;

        for (int currentDepth = 0; currentDepth <= lavaLineDepth - 140; currentDepth++)
        {
            progress.Set((double)currentDepth / (double)(lavaLineDepth - 140));
            leftEdge += WorldGen.genRand.Next(-4, 4);
            rightEdge += WorldGen.genRand.Next(-3, 5);

            if (currentDepth > 0)
            {
                leftEdge = (leftEdge + GenVars.snowMinX[currentDepth - 1]) / 2;
                rightEdge = (rightEdge + GenVars.snowMaxX[currentDepth - 1]) / 2;
            }

            if (GenVars.dungeonSide > 0)
            {
                if (WorldGen.genRand.NextBool(4))
                {
                    leftEdge++;
                    rightEdge++;
                }
            }
            else if (WorldGen.genRand.NextBool(4))
            {
                leftEdge--;
                rightEdge--;
            }

            GenVars.snowMinX[currentDepth] = leftEdge;
            GenVars.snowMaxX[currentDepth] = rightEdge;

            for (int x = leftEdge; x < rightEdge; x++)
            {
                if (currentDepth < snowDepthBound)
                {
                    // Create standard tundra biome for ALL regions
                    if (Main.tile[x, currentDepth].WallType == 2)
                        Main.tile[x, currentDepth].WallType = 40;

                    Tile tile = Main.tile[x, currentDepth];
                    if (IsValidCoordinate(x, currentDepth) && tile.HasTile)
                    {
                        bool isWithinSurfaceLayer = currentDepth <= (int)Main.worldSurface + 8;
                        switch (tile.TileType)
                        {
                            case TileID.Dirt:
                            case TileID.Grass:
                            case TileID.Mud:
                            case TileID.JungleGrass:
                            case TileID.Sand:
                                tile.TileType = TileID.SnowBlock;
                                break;
                            case TileID.Stone:
                                tile.TileType = TileID.IceBlock;
                                break;
                        }

                        if (isWithinSurfaceLayer && !IsInSnowCenter(x))
                        {
                            switch (tile.TileType)
                            {
                                case TileID.Dirt:
                                case TileID.Grass:
                                case TileID.Mud:
                                case TileID.JungleGrass:
                                case TileID.Sand:
                                case TileID.SnowBlock:
                                    tile.TileType = (ushort)ModContent.TileType<PeatTile>();
                                    break;
                                case TileID.Stone:
                                case TileID.IceBlock:
                                    tile.TileType = TileID.SnowBlock;
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    columnDepth += WorldGen.genRand.Next(-3, 4);
                    if (WorldGen.genRand.NextBool(3))
                    {
                        columnDepth += WorldGen.genRand.Next(-4, 5);
                        if (WorldGen.genRand.NextBool(3))
                            columnDepth += WorldGen.genRand.Next(-6, 7);
                    }

                    if (columnDepth < 0)
                        columnDepth = WorldGen.genRand.Next(3);
                    else if (columnDepth > 50)
                        columnDepth = 50 - WorldGen.genRand.Next(3);

                    // For deep areas
                    for (int columnY = currentDepth; columnY < currentDepth + columnDepth; columnY++)
                    {
                        if (Main.tile[x, columnY].WallType == 2)
                            Main.tile[x, columnY].WallType = 40;
                    }
                }
            }

            if (GenVars.snowBottom < currentDepth)
                GenVars.snowBottom = currentDepth;
        }
    }

    private void ApplyTaigaSurface()
    {
        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;
        int tundraWidth = rightEdge - leftEdge;

        // Randomize the widths of tundra and taiga biomes
        int snowWidthPercent = WorldGen.genRand.Next(44, 57);
        int snowCoreStart = leftEdge + (tundraWidth * (100 - snowWidthPercent) / 200);
        int snowCoreEnd = rightEdge - (tundraWidth * (100 - snowWidthPercent) / 200);

        for (int x = leftEdge; x < rightEdge; x++)
        {
            int surfaceY = FindSurfaceLevel(x);
            if (surfaceY <= 0)
                continue;

            // Apply taiga or snow surface based on randomized widths  
            if (x >= snowCoreStart && x <= snowCoreEnd)
            {
                // Snow surface
                Main.tile[x, surfaceY].TileType = TileID.SnowBlock;
            }
            else
            {
                // Taiga surface
                Main.tile[x, surfaceY].TileType = (ushort)ModContent.TileType<PeatTile>();
            }
        }
    }

    private void AddTaigaDetails()
    {
        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;

        // Create peat and stone patches for additional detail
        for (int x = leftEdge; x < rightEdge; x += WorldGen.genRand.Next(5, 15))
        {
            if (IsInSnowCenter(x))
                continue;

            int surfaceY = FindSurfaceLevel(x);
            if (surfaceY <= 0)
                continue;

            // Create some stone patches
            if (WorldGen.genRand.NextBool(3))
            {
                WorldGen.TileRunner(
                    x,
                    surfaceY + WorldGen.genRand.Next(3, 8),
                    WorldGen.genRand.Next(3, 7),
                    WorldGen.genRand.Next(10, 20),
                    TileID.Stone,
                    true
                );
            }

            // Create peat pockets near the surface
            if (WorldGen.genRand.NextBool(3))
            {
                WorldGen.TileRunner(
                    x,
                    surfaceY + WorldGen.genRand.Next(2, 6),
                    WorldGen.genRand.Next(4, 8),
                    WorldGen.genRand.Next(10, 20),
                    ModContent.TileType<PeatTile>(),
                    true
                );
            }

            // Add natural snow patches (not ice)
            if (WorldGen.genRand.NextBool(4))
            {
                WorldGen.TileRunner(
                    x,
                    surfaceY,
                    WorldGen.genRand.Next(3, 8),
                    WorldGen.genRand.Next(5, 15),
                    TileID.SnowBlock,
                    true
                );
            }

            // Add small puddles occasionally
            if (WorldGen.genRand.NextBool(10))
            {
                int puddleX = x + WorldGen.genRand.Next(-10, 11);
                int puddleY = FindSurfaceLevel(puddleX);

                if (puddleY > 0)
                {
                    WorldGen.TileRunner(
                        puddleX,
                        puddleY + 1,
                        WorldGen.genRand.Next(2, 5),
                        WorldGen.genRand.Next(3, 8),
                        -1, // -1 for liquid
                        false,
                        0, 0,
                        false,
                        true
                    );

                    // Add water
                    for (int px = puddleX - 3; px <= puddleX + 3; px++)
                    {
                        for (int py = puddleY - 1; py <= puddleY + 4; py++)
                        {
                            if (!IsValidCoordinate(px, py))
                                continue;
                            Tile tile = Main.tile[px, py];
                            if (!Main.tile[px, py].HasTile)
                            {
                                tile.LiquidAmount = 255;
                                tile.LiquidType = 0; // Water
                            }
                        }
                    }
                }
            }
        }
    }

    private void BlendBiomes()
    {
        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;
        int tundraWidth = rightEdge - leftEdge;

        int snowWidthPercent = WorldGen.genRand.Next(44, 57);
        int snowCoreStart = leftEdge + (tundraWidth * (100 - snowWidthPercent) / 200);
        int snowCoreEnd = rightEdge - (tundraWidth * (100 - snowWidthPercent) / 200);

        // Define blending zone width
        int blendingWidth = (int)(tundraWidth * 0.1f);

        for (int x = snowCoreStart - blendingWidth; x <= snowCoreEnd + blendingWidth; x++)
        {
            if (!IsValidCoordinate(x, 0))
                continue;

            int surfaceY = FindSurfaceLevel(x);
            if (surfaceY <= 0)
                continue;

            int maxDepth = 10;
            for (int depth = 1; depth <= maxDepth; depth++)
            {
                int y = surfaceY + depth;
                if (!IsValidCoordinate(x, y))
                    continue;

                Tile tile = Main.tile[x, y];
                if (!tile.HasTile)
                    continue;

                // Calculate blending factor based on distance from biome edges
                float distanceFromEdge = Math.Min(
                    Math.Abs(x - (snowCoreStart - blendingWidth)),
                    Math.Abs(x - (snowCoreEnd + blendingWidth))
                );
                float blendFactor = distanceFromEdge / blendingWidth;

                // Apply blending between snow and peat
                if (WorldGen.genRand.NextFloat() < blendFactor)
                {
                    tile.TileType = TileID.SnowBlock;
                }
                else
                {
                    tile.TileType = (ushort)ModContent.TileType<PeatTile>();
                }
            }
        }
    }

    private bool IsInSnowCenter(int x)
    {
        int centerX = (GenVars.snowOriginLeft + GenVars.snowOriginRight) / 2;
        int tundraWidth = GenVars.snowOriginRight - GenVars.snowOriginLeft;
        int snowWidthPercent = 56;
        int snowStart = centerX - (tundraWidth * snowWidthPercent / 200);
        int snowEnd = centerX + (tundraWidth * snowWidthPercent / 200);

        return x >= snowStart && x <= snowEnd;
    }

    private bool IsSurfaceLevel(int x, int y)
    {
        return IsValidCoordinate(x, y) &&
               Main.tile[x, y].HasTile &&
               (!IsValidCoordinate(x, y - 1) || !Main.tile[x, y - 1].HasTile);
    }

    private int FindSurfaceLevel(int x)
    {
        // Start search from near world surface level
        for (int y = (int)Main.worldSurface - 50; y < (int)Main.worldSurface + 50; y++)
        {
            if (IsSurfaceLevel(x, y))
                return y;
        }

        return -1; // No surface found
    }

    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    }
}