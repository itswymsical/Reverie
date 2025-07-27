using Terraria.IO;
using Terraria.WorldBuilding;
using Reverie.Content.Tiles.Taiga;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TaigaPass : GenPass
{
    public TaigaPass() : base("[Reverie] Taiga Biome", 247.43f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing Taiga";
        GenerateTaiga(progress);
    }
    private static void GenerateTaiga(GenerationProgress progress)
    {
        int depth = (int)Main.worldSurface + 100;

        // Calculate taiga position between dungeon and tundra
        int taigaLeft, taigaRight;

        if (TryPlaceBetweenDungeonAndTundra(out taigaLeft, out taigaRight))
        {
            // Successfully positioned between dungeon and tundra
        }
        else
        {
            if (!FindSuitableTerrainLocation(out taigaLeft, out taigaRight))
            {
                PlaceOppositeFromDungeon(out taigaLeft, out taigaRight);
            }
        }

        for (int currentDepth = 0; currentDepth <= depth; currentDepth++)
        {
            progress.Set((double)currentDepth / depth);

            taigaLeft += WorldGen.genRand.Next(-2, 3);
            taigaRight += WorldGen.genRand.Next(-2, 3);

            if (currentDepth > 0)
            {
                taigaLeft = (taigaLeft + GenVars.snowMinX[currentDepth - 1]) / 2;
                taigaRight = (taigaRight + GenVars.snowMaxX[currentDepth - 1]) / 2;
            }

            GenVars.snowMinX[currentDepth] = taigaLeft;
            GenVars.snowMaxX[currentDepth] = taigaRight;

            for (int x = taigaLeft; x < taigaRight; x++)
            {
                if (IsValidCoordinate(x, currentDepth))
                {
                    Tile tile = Main.tile[x, currentDepth];
                    if (tile.TileType == TileID.Dirt)
                    {
                        tile.TileType = (ushort)ModContent.TileType<PeatTile>();
                    }
                    else if (tile.TileType == TileID.Grass)
                    {
                        tile.TileType = (ushort)ModContent.TileType<SnowTaigaGrassTile>();
                    }
                    else if (tile.TileType == TileID.ClayBlock)
                    {
                        tile.TileType = TileID.IceBlock;
                    }
                    else if (tile.TileType == 3)
                    {
                        tile.TileType = (ushort)ModContent.TileType<TaigaPlants>();
                    }
                    else if (tile.TileType == 24)
                    {
                        tile.TileType = (ushort)ModContent.TileType<CorruptTaigaPlants>();
                    }
                    else if (tile.TileType == 201)
                    {
                        tile.TileType = (ushort)ModContent.TileType<CrimsonTaigaPlants>();
                    }
                    else if (tile.TileType == TileID.Sand || tile.TileType == TileID.Crimsand || tile.TileType == TileID.Ebonsand)
                    {
                        tile.TileType = (ushort)TileID.SnowBlock;
                    }
                    else if (tile.TileType == TileID.CrimsonGrass)
                    {
                        tile.TileType = (ushort)ModContent.TileType<CrimsonTaigaGrassTile>();
                    }
                    else if (tile.TileType == TileID.CorruptGrass)
                    {
                        tile.TileType = (ushort)ModContent.TileType<CorruptTaigaGrassTile>();
                    }

                    if (tile.WallType == WallID.FlowerUnsafe || tile.WallType == WallID.GrassUnsafe)
                    {
                        tile.WallType = WallID.DirtUnsafe;
                    }
                }
            }
        }
    }

    private static bool TryPlaceBetweenDungeonAndTundra(out int taigaLeft, out int taigaRight)
    {
        int dungeonX = GenVars.dungeonX;
        int tundraLeft = GenVars.snowOriginLeft;
        int tundraRight = GenVars.snowOriginRight;

        // Calculate taiga width relative to world size
        int taigaWidth = (int)(Main.maxTilesX * 0.061f);

        // Use the appropriate tundra edge based on dungeon side
        int tundraEdge;
        if (GenVars.dungeonSide < 0) // Dungeon on left
        {
            tundraEdge = tundraRight; // Use right edge of tundra
        }
        else // Dungeon on right
        {
            tundraEdge = tundraLeft; // Use left edge of tundra
        }

        // Calculate space between dungeon and tundra
        int availableSpace = Math.Abs(dungeonX - tundraEdge);
        int minRequiredSpace = taigaWidth + 100; // Need taiga width plus buffers

        if (availableSpace < minRequiredSpace)
        {
            taigaLeft = taigaRight = 0;
            return false;
        }

        // Place taiga in middle area between dungeon and tundra
        if (GenVars.dungeonSide < 0) // Dungeon on left, tundra on right
        {
            int centerPoint = (dungeonX + tundraEdge) / 2;
            taigaLeft = centerPoint - taigaWidth / 2;
            taigaRight = centerPoint + taigaWidth / 2;
        }
        else // Dungeon on right, tundra on left
        {
            int centerPoint = (tundraEdge + dungeonX) / 2;
            taigaLeft = centerPoint - taigaWidth / 2;
            taigaRight = centerPoint + taigaWidth / 2;
        }

        // Ensure we stay within reasonable bounds
        taigaLeft = Math.Max(taigaLeft, 100);
        taigaRight = Math.Min(taigaRight, Main.maxTilesX - 100);

        return true;
    }

    private static bool FindSuitableTerrainLocation(out int taigaLeft, out int taigaRight)
    {
        int taigaWidth = (int)(Main.maxTilesX * 0.011f);

        // Scan surface for suitable areas
        for (int startX = 200; startX < Main.maxTilesX - taigaWidth - 200; startX += 50)
        {
            // Skip if overlaps with existing biomes
            if (IsNearExistingBiome(startX, taigaWidth))
                continue;

            // Check if this area has suitable terrain
            if (IsSuitableTerrain(startX, taigaWidth))
            {
                taigaLeft = startX;
                taigaRight = startX + taigaWidth;
                return true;
            }
        }

        taigaLeft = taigaRight = 0;
        return false;
    }

    private static bool IsNearExistingBiome(int startX, int width)
    {
        int surfaceY = (int)Main.worldSurface;
        int samplePoints = 10;

        for (int i = 0; i < samplePoints; i++)
        {
            int x = startX + (width * i / samplePoints);
            if (!IsValidCoordinate(x, surfaceY)) continue;

            Tile tile = Main.tile[x, surfaceY];

            // Check for existing biome indicators
            if (tile.TileType == TileID.Sand || tile.TileType == TileID.HardenedSand ||
                tile.TileType == TileID.JungleGrass || tile.TileType == TileID.Mud ||
                tile.TileType == TileID.SnowBlock || tile.TileType == TileID.IceBlock)
            {
                return true;
            }
        }

        // Check proximity to tundra
        int tundraDistance = Math.Min(Math.Abs(startX - GenVars.snowOriginLeft),
                                      Math.Abs(startX + width - GenVars.snowOriginRight));
        if (tundraDistance < 100)
            return true;

        return false;
    }

    private static bool IsSuitableTerrain(int startX, int width)
    {
        int surfaceY = (int)Main.worldSurface;
        int suitableCount = 0;
        int totalSamples = 20;

        for (int i = 0; i < totalSamples; i++)
        {
            int x = startX + (width * i / totalSamples);
            if (!IsValidCoordinate(x, surfaceY)) continue;

            Tile tile = Main.tile[x, surfaceY];

            // Count tiles that are good for taiga conversion
            if (tile.TileType == TileID.Dirt || tile.TileType == TileID.Grass ||
                tile.TileType == TileID.Stone || tile.TileType == TileID.ClayBlock
                || tile.TileType == TileID.Ebonsand || tile.TileType == TileID.Ebonstone
                || tile.TileType == TileID.CorruptGrass || tile.TileType == TileID.Crimsand || tile.TileType == TileID.Crimstone
                || tile.TileType == TileID.CrimsonGrass)
            {
                suitableCount++;
            }
        }

        // Need at least 70% suitable terrain
        return (double)suitableCount / totalSamples >= 0.7;
    }

    private static void PlaceOppositeFromDungeon(out int taigaLeft, out int taigaRight)
    {
        // Place taiga on opposite side of world from dungeon
        int dungeonX = GenVars.dungeonX;
        int worldCenter = Main.maxTilesX / 2;
        int taigaWidth = (int)(Main.maxTilesX * 0.011f);

        if (dungeonX < worldCenter)
        {
            // Dungeon on left, place taiga on right side
            taigaLeft = (int)(Main.maxTilesX * 0.75) - taigaWidth / 2;
        }
        else
        {
            // Dungeon on right, place taiga on left side  
            taigaLeft = (int)(Main.maxTilesX * 0.25) - taigaWidth / 2;
        }

        taigaRight = taigaLeft + taigaWidth;

        // Ensure boundaries are within world limits
        if (taigaLeft < 100) taigaLeft = 100;
        if (taigaRight > Main.maxTilesX - 100) taigaRight = Main.maxTilesX - 100;
    }
    private static bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    }
}
