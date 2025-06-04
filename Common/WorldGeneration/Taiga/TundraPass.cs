using Reverie.Content.Tiles.Taiga;
using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TundraPass : GenPass
{
    public TundraPass() : base("[Reverie] Tundra Biome", 247.43f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating Ice Biome";
        GenerateTundra(progress);

        progress.Message = "Growing Taiga";
        ApplyTaigaSurface();

        progress.Message = "Foresting the Taiga";
        SpreadGrass();
        progress.Set(1.0f);
    }

    private static void GenerateTundra(GenerationProgress progress)
    {
        GenVars.snowTop = (int)Main.worldSurface;
        int lavaLineDepth = GenVars.lavaLine;

        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;

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
                if (IsValidCoordinate(x, currentDepth))
                {
                    Tile tile = Main.tile[x, currentDepth];
                    tile.TileType = (ushort)ModContent.TileType<PeatTile>();
                }
            }
        }

        GenVars.snowBottom = (int)Main.worldSurface;
    }

    private static void ApplyTaigaSurface()
    {
        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;
        int snowWidth = rightEdge - leftEdge;
        int worldCenter = Main.maxTilesX / 2;
        int taigaWidth = snowWidth * 2; // Make taiga twice as wide as snow biome

        // Determine if snow biome is more to the left or right of world center
        int snowCenter = leftEdge + (snowWidth / 2);
        bool placeOnLeft = snowCenter > worldCenter;

        // Calculate taiga boundaries based on position
        int taigaLeft, taigaRight;
        if (placeOnLeft)
        {
            // Place taiga to the left of snow
            taigaRight = leftEdge;
            taigaLeft = taigaRight - taigaWidth;
        }
        else
        {
            // Place taiga to the right of snow
            taigaLeft = rightEdge;
            taigaRight = taigaLeft + taigaWidth;
        }

        // Ensure we don't go outside world bounds
        taigaLeft = Math.Max(0, taigaLeft);
        taigaRight = Math.Min(Main.maxTilesX - 1, taigaRight);

        // Place taiga tiles
        for (int x = taigaLeft; x < taigaRight; x++)
        {
            int surfaceY = FindSurfaceLevel(x);
            if (surfaceY <= 0)
                continue;

            Main.tile[x, surfaceY].TileType = (ushort)ModContent.TileType<PeatTile>();
        }
    }

    private static void SpreadGrass()
    {
        // Calculate taiga boundaries (same logic as ApplyTaigaSurface)
        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;
        int snowWidth = rightEdge - leftEdge;
        int worldCenter = Main.maxTilesX / 2;
        int taigaWidth = snowWidth * 2;

        int snowCenter = leftEdge + (snowWidth / 2);
        bool placeOnLeft = snowCenter > worldCenter;

        // Calculate taiga boundaries based on position
        int taigaLeft, taigaRight;
        if (placeOnLeft)
        {
            taigaRight = leftEdge;
            taigaLeft = taigaRight - taigaWidth;
        }
        else
        {
            taigaLeft = rightEdge;
            taigaRight = taigaLeft + taigaWidth;
        }

        // Ensure we don't go outside world bounds
        taigaLeft = Math.Max(5, taigaLeft);
        taigaRight = Math.Min(Main.maxTilesX - 5, taigaRight);

        // Only spread grass within taiga boundaries
        for (var x = taigaLeft; x < taigaRight; x++)
        {
            for (var y = 5; y < Main.maxTilesY - 5; y++)
            {
                var tile = Framing.GetTileSafely(x, y);
                var tileAbove = Framing.GetTileSafely(x, y - 1);
                var tileBelow = Framing.GetTileSafely(x, y + 1);

                if (tile.TileType == (ushort)ModContent.TileType<PeatTile>() && !tileAbove.HasTile)
                {
                    tile.TileType = (ushort)ModContent.TileType<SnowTaigaGrassTile>();

                    // Spread to neighboring tiles within taiga boundaries
                    for (var grassX = Math.Max(taigaLeft, x - 1); grassX <= Math.Min(taigaRight - 1, x + 1); grassX++)
                    {
                        for (var grassY = y - 1; grassY < y + 1; grassY++)
                        {
                            if (grassY >= 0 && grassY < Main.maxTilesY)
                            {
                                var currentTile = Framing.GetTileSafely(grassX, grassY);
                                var grassAbove = Framing.GetTileSafely(grassX, grassY - 1);
                                if (currentTile.HasTile && currentTile.TileType == (ushort)ModContent.TileType<PeatTile>() && !grassAbove.HasTile)
                                {
                                    currentTile.TileType = (ushort)ModContent.TileType<SnowTaigaGrassTile>();
                                    WorldGen.SpreadGrass(grassX, grassY, (ushort)ModContent.TileType<PeatTile>(), (ushort)ModContent.TileType<SnowTaigaGrassTile>(), true);
                                }
                            }
                        }
                    }
                }

                if (WorldGen.genRand.NextBool(3) && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
                {
                    if (y > 1)
                    {
                        WorldGen.PlaceTile(x, y - 1, (ushort)ModContent.TileType<SnowTaigaPlants>(), mute: true);
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
            }
        }
    }

    private static bool IsSurfaceLevel(int x, int y)
    {
        return IsValidCoordinate(x, y) &&
               Main.tile[x, y].HasTile &&
               (!IsValidCoordinate(x, y - 1) || !Main.tile[x, y - 1].HasTile);
    }

    private static int FindSurfaceLevel(int x)
    {
        for (int y = (int)Main.worldSurface - 50; y < (int)Main.worldSurface + 50; y++)
        {
            if (IsSurfaceLevel(x, y))
                return y;
        }

        return -1;
    }

    private static bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    }
}