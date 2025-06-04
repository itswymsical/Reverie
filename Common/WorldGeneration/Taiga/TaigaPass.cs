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
        int depth = (int)Main.worldSurface + 140;

        // Calculate taiga position based on dungeon side and spawn point
        int taigaLeft, taigaRight;
        int spawnPoint = Main.spawnTileX;

        if (GenVars.dungeonSide > 0) // Dungeon on right
        {
            // Place taiga between spawn and snow on left side
            taigaRight = GenVars.snowOriginLeft;
            taigaLeft = (spawnPoint + taigaRight) / 2; // Midpoint between spawn and snow
        }
        else // Dungeon on left
        {
            // Place taiga between spawn and snow on right side
            taigaLeft = GenVars.snowOriginRight;
            taigaRight = (spawnPoint + taigaLeft) / 2; // Midpoint between spawn and snow
        }

        // Ensure minimum width
        int minimumWidth = 100;
        if (Math.Abs(taigaRight - taigaLeft) < minimumWidth)
        {
            if (GenVars.dungeonSide > 0)
                taigaLeft = taigaRight - minimumWidth;
            else
                taigaRight = taigaLeft + minimumWidth;
        }

        for (int currentDepth = 0; currentDepth <= depth; currentDepth++)
        {
            progress.Set((double)currentDepth / depth);

            // Add some noise to the edges
            taigaLeft += WorldGen.genRand.Next(-2, 3);
            taigaRight += WorldGen.genRand.Next(-2, 3);

            if (currentDepth > 0)
            {
                // Smooth the edges with previous layer
                taigaLeft = (taigaLeft + GenVars.snowMinX[currentDepth - 1]) / 2;
                taigaRight = (taigaRight + GenVars.snowMaxX[currentDepth - 1]) / 2;
            }

            // Store the boundaries
            GenVars.snowMinX[currentDepth] = taigaLeft;
            GenVars.snowMaxX[currentDepth] = taigaRight;

            // Place taiga tiles
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

    private static bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    }
}
