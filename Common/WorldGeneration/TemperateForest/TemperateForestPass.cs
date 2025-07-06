using Reverie.Content.Tiles.Taiga;
using Reverie.Content.Tiles.TemperateForest;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.TemperateForest;

public class TemperateForestPass : GenPass
{
    public TemperateForestPass() : base("[Reverie] Temperate Forest", 249f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing Temperate Forest";
        GenerateTemperateForest(progress);
    }

    private static void GenerateTemperateForest(GenerationProgress progress)
    {
        int depth = (int)Main.worldSurface + 100;

        // Calculate forest position near spawn
        int forestLeft, forestRight;

        if (TryPlaceNearSpawn(out forestLeft, out forestRight))
        {
            // Successfully positioned near spawn
        }
        else
        {
            // Find suitable terrain elsewhere
            if (!FindSuitableTerrainLocation(out forestLeft, out forestRight))
            {
                // Final fallback: place on less populated side of world
                PlaceOnQuieterSide(out forestLeft, out forestRight);
            }
        }

        // Generate the forest layer by layer
        for (int currentDepth = 0; currentDepth <= depth; currentDepth++)
        {
            progress.Set((double)currentDepth / depth);

            // Add natural edge variation
            forestLeft += WorldGen.genRand.Next(-2, 3);
            forestRight += WorldGen.genRand.Next(-2, 3);

            if (currentDepth > 0)
            {
                // Smooth edges with previous layer
                forestLeft = (forestLeft + GenVars.snowMinX[currentDepth - 1]) / 2;
                forestRight = (forestRight + GenVars.snowMaxX[currentDepth - 1]) / 2;
            }

            // Store boundaries for edge smoothing
            GenVars.snowMinX[currentDepth] = forestLeft;
            GenVars.snowMaxX[currentDepth] = forestRight;

            // Convert tiles to temperate forest
            for (int x = forestLeft; x < forestRight; x++)
            {
                if (IsValidCoordinate(x, currentDepth))
                {
                    Tile tile = Main.tile[x, currentDepth];
                    if (tile.TileType == TileID.Grass || tile.TileType == TileID.CrimsonGrass || tile.TileType == TileID.CorruptGrass)
                    {
                        tile.TileType = (ushort)ModContent.TileType<TemperateGrassTile>();
                    }
                    else if (tile.TileType == TileID.Plants2 || tile.TileType == TileID.Plants)
                    {
                        tile.TileType = (ushort)ModContent.TileType<TemperatePlants>();
                    }
                }
            }
        }
    }

    private static bool TryPlaceNearSpawn(out int forestLeft, out int forestRight)
    {
        int spawnX = Main.spawnTileX;
        int forestWidth = (int)(Main.maxTilesX * 0.05f);
        int minDistance = (int)(Main.maxTilesX * 0.00065f);
        int maxDistance = (int)(Main.maxTilesX * 0.02f);

        // Try placing on both sides of spawn
        bool leftSideClear = CheckSideForPlacement(spawnX - maxDistance, spawnX - minDistance, forestWidth);
        bool rightSideClear = CheckSideForPlacement(spawnX + minDistance, spawnX + maxDistance, forestWidth);

        if (!leftSideClear && !rightSideClear)
        {
            forestLeft = forestRight = 0;
            return false;
        }

        // Prefer the side with less existing biomes
        if (leftSideClear && rightSideClear)
        {
            // Check which side has fewer biome conflicts
            bool preferLeft = WorldGen.genRand.NextBool();

            if (preferLeft)
            {
                forestLeft = spawnX - minDistance - forestWidth;
                forestRight = spawnX - minDistance;
            }
            else
            {
                forestLeft = spawnX + minDistance;
                forestRight = spawnX + minDistance + forestWidth;
            }
        }
        else if (leftSideClear)
        {
            forestLeft = spawnX - minDistance - forestWidth;
            forestRight = spawnX - minDistance;
        }
        else
        {
            forestLeft = spawnX + minDistance;
            forestRight = spawnX + minDistance + forestWidth;
        }

        // Ensure bounds are within world limits
        forestLeft = Math.Max(forestLeft, 100);
        forestRight = Math.Min(forestRight, Main.maxTilesX - 100);

        return true;
    }

    private static bool CheckSideForPlacement(int startX, int endX, int forestWidth)
    {
        // Ensure we're within world bounds
        if (startX < 100 || endX > Main.maxTilesX - 100) return false;

        // Check for existing biomes in this area
        return !IsNearExistingBiome(startX, forestWidth);
    }

    private static bool FindSuitableTerrainLocation(out int forestLeft, out int forestRight)
    {
        int forestWidth = (int)(Main.maxTilesX * 0.011f);

        // Scan surface for suitable areas
        for (int startX = 200; startX < Main.maxTilesX - forestWidth - 200; startX += 50)
        {
            // Skip if overlaps with existing biomes
            if (IsNearExistingBiome(startX, forestWidth))
                continue;

            // Check if this area has suitable terrain
            if (IsSuitableTerrain(startX, forestWidth))
            {
                forestLeft = startX;
                forestRight = startX + forestWidth;
                return true;
            }
        }

        forestLeft = forestRight = 0;
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
                tile.TileType == TileID.SnowBlock || tile.TileType == TileID.IceBlock ||
                tile.TileType == (ushort)ModContent.TileType<PeatTile>())
            {
                return true;
            }
        }

        // Check proximity to snow biome
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

            // Count tiles that are good for forest conversion
            if (tile.TileType == TileID.Dirt || tile.TileType == TileID.Grass ||
                tile.TileType == TileID.Stone || tile.TileType == TileID.ClayBlock)
            {
                suitableCount++;
            }
        }

        // Need at least 70% suitable terrain
        return (double)suitableCount / totalSamples >= 0.7;
    }

    private static void PlaceOnQuieterSide(out int forestLeft, out int forestRight)
    {
        // Place forest on side with fewer existing biomes
        int dungeonX = GenVars.dungeonX;
        int worldCenter = Main.maxTilesX / 2;
        int forestWidth = (int)(Main.maxTilesX * 0.011f);

        // Place opposite from dungeon to spread biomes out
        if (dungeonX < worldCenter)
        {
            // Dungeon on left, place forest on right side
            forestLeft = (int)(Main.maxTilesX * 0.7) - forestWidth / 2;
        }
        else
        {
            // Dungeon on right, place forest on left side  
            forestLeft = (int)(Main.maxTilesX * 0.3) - forestWidth / 2;
        }

        forestRight = forestLeft + forestWidth;

        // Ensure boundaries are within world limits
        if (forestLeft < 100) forestLeft = 100;
        if (forestRight > Main.maxTilesX - 100) forestRight = Main.maxTilesX - 100;
    }

    private static bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    }
}