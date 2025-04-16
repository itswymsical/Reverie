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

        progress.Message = "Blending Biomes";
        BlendBiomes();

        progress.Message = "Foresting the Taiga";
        SpreadGrass();
        progress.Set(1.0f);
    }

    private static void GenerateTundra(GenerationProgress progress)
    {
        GenVars.snowTop = (int)Main.worldSurface;
        int snowDepthBound = GenVars.lavaLine - WorldGen.genRand.Next(160, 200);
        int lavaLineDepth = GenVars.lavaLine;

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

            int snowWidthPercent = WorldGen.genRand.Next(44, 57);
            int snowCoreStart = leftEdge + (rightEdge - leftEdge) * (100 - snowWidthPercent) / 200;
            int snowCoreEnd = rightEdge - (rightEdge - leftEdge) * (100 - snowWidthPercent) / 200;

            for (int x = leftEdge; x < rightEdge; x++)
            {
                if (currentDepth < snowDepthBound)
                {
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
                    }
                }

                Tile tile2 = Main.tile[x, currentDepth];
                if (x >= snowCoreStart && x <= snowCoreEnd)
                {                   
                    if (currentDepth <= (int)Main.worldSurface + 8)
                    {           
                        switch (tile2.TileType)
                        {
                            case TileID.Dirt:
                            case TileID.Grass:
                            case TileID.Mud:
                            case TileID.Sand:
                                tile2.TileType = TileID.SnowBlock;
                                break;
                            case TileID.Stone:
                            case TileID.ClayBlock:
                                tile2.TileType = TileID.IceBlock;
                                break;
                        }
                    }
                }
                else
                {
                    if (currentDepth <= (int)Main.worldSurface + 8)
                    {
                        switch (tile2.TileType)
                        {
                            case TileID.Dirt:
                            case TileID.Grass:
                            case TileID.Mud:
                            case TileID.Sand:
                            case TileID.SnowBlock:
                                tile2.TileType = (ushort)ModContent.TileType<PeatTile>();
                                break;
                            case TileID.Stone:
                            case TileID.IceBlock:
                                tile2.TileType = TileID.SnowBlock;
                                break;
                        }
                    }
                }

                if (currentDepth > snowDepthBound)
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

    private static void ApplyTaigaSurface()
    {
        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;
        int tundraWidth = rightEdge - leftEdge;

        int snowWidthPercent = WorldGen.genRand.Next(44, 57);
        int snowCoreStart = leftEdge + (tundraWidth * (100 - snowWidthPercent) / 200);
        int snowCoreEnd = rightEdge - (tundraWidth * (100 - snowWidthPercent) / 200);

        for (int x = leftEdge; x < rightEdge; x++)
        {
            int surfaceY = FindSurfaceLevel(x);
            if (surfaceY <= 0)
                continue;

            if (x >= snowCoreStart && x <= snowCoreEnd)
            {
                Main.tile[x, surfaceY].TileType = TileID.SnowBlock;
            }
            else
            {
                Main.tile[x, surfaceY].TileType = (ushort)ModContent.TileType<PeatTile>();
            }
        }
    }

    private static void BlendBiomes()
    {
        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;
        int tundraWidth = rightEdge - leftEdge;

        int snowWidthPercent = WorldGen.genRand.Next(44, 57);
        int snowCoreStart = leftEdge + (tundraWidth * (93 - snowWidthPercent) / 187);
        int snowCoreEnd = rightEdge - (tundraWidth * (93 - snowWidthPercent) / 187);

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

                float distanceFromEdge = Math.Min(
                    Math.Abs(x - (snowCoreStart - blendingWidth)),
                    Math.Abs(x - (snowCoreEnd + blendingWidth))
                );
                float blendFactor = distanceFromEdge / blendingWidth;

                if (WorldGen.genRand.NextFloat() > blendFactor)
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

    private static void SpreadGrass()
    {
        //idgaf, checking the entire world bc I had enough of trying to get both sides of the taiga to generate grass
        for (var x = 5; x < Main.maxTilesX - 5; x++)
        {
            for (var y = 5; y < Main.maxTilesY - 5; y++)
            {
                var tile = Framing.GetTileSafely(x, y);
                var tileAbove = Framing.GetTileSafely(x, y - 1);
                var tileBelow = Framing.GetTileSafely(x, y + 1);

                if (tile.TileType == (ushort)ModContent.TileType<PeatTile>() && !tileAbove.HasTile)
                {
                    tile.TileType = (ushort)ModContent.TileType<SnowTaigaGrassTile>();

                    for (var grassX = x - 1; grassX < x + 1; grassX++) // Changed < to <= to include x+1
                    {
                        for (var grassY = y - 1; grassY < y + 1; grassY++) // Changed < to <= to include y+1
                        {
                            if (grassX >= 0 && grassX < Main.maxTilesX && grassY >= 0 && grassY < Main.maxTilesY)
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