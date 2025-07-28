using Reverie.Content.Tiles.Canopy.Trees;
using Reverie.Content.Tiles.Taiga;
using Reverie.Content.Tiles.Taiga.Trees;
using System.Collections.Generic;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TaigaDecorPass : GenPass
{
    public TaigaDecorPass() : base("[Reverie] Taiga Grass", 248f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Decorating cold biomes...";
        SpreadGrass(progress);
    }


    #region Core Methods
    private void SpreadGrass(GenerationProgress progress)
    {
        var peatTiles = FindSoilTiles();
        if (peatTiles.Count == 0)
        {
            return;
        }

        var processed = 0;
        foreach (var peatPos in peatTiles)
        {
            processed++;
            if (processed % 100 == 0)
            {
                progress.Set((double)processed / peatTiles.Count);
            }
            SpreadGrassAt(peatPos.X, peatPos.Y);
        }
    }

    private List<Point> FindSoilTiles()
    {
        var peatTiles = new List<Point>();
        for (var x = 100; x < Main.maxTilesX - 100; x++)
        {
            for (var y = 50; y < Main.maxTilesY - 100; y++)
            {
                if (!WorldGen.InWorld(x, y)) continue;
                var tile = Main.tile[x, y];
                if (tile.HasTile && tile.TileType == (ushort)ModContent.TileType<PeatTile>())
                {
                    peatTiles.Add(new Point(x, y));
                }
            }
        }
        return peatTiles;
    }

    private void SpreadGrassAt(int x, int y)
    {
        var tile = Framing.GetTileSafely(x, y);
        if (!tile.HasTile || tile.TileType != (ushort)ModContent.TileType<PeatTile>())
            return;

        if (IsExposedToAir(x, y))
        {
            PopulateBiome(x, y);
        }
    }

    private bool IsExposedToAir(int x, int y)
    {
        for (var checkX = x - 1; checkX <= x + 1; checkX++)
        {
            for (var checkY = y - 1; checkY <= y + 1; checkY++)
            {
                if (checkX == x && checkY == y) continue;
                if (!WorldGen.InWorld(checkX, checkY)) continue;
                var neighborTile = Framing.GetTileSafely(checkX, checkY);
                if (!neighborTile.HasTile)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void PopulateBiome(int x, int y)
    {
        var tile = Framing.GetTileSafely(x, y);
        if (!tile.HasTile) return;

        var grassType = GetGrassType(x, y);
        tile.TileType = grassType;

        if (Main.rand.NextBool(10) && HasSpacing(x, y, 3))
        {
            SpruceTree.GrowSpruceTree(x, y - 1);
        }

        if (Main.rand.NextBool(2))
        {
            WorldGen.PlaceTile(x, y - 1, GetPlantType(grassType), style: Main.rand.Next(18));
        }

        if (Main.rand.NextBool(16) && HasSpacing(x, y, 1))
        {
            var styleRange = WinterberryStyles(grassType);
            var style = Main.rand.Next(styleRange.start, styleRange.end + 1);
            WorldGen.PlaceTile(x, y - 1, ModContent.TileType<WinterberryBushTile>(), style: style);
        }



        WorldGen.SquareTileFrame(x, y, true);
        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
        }
    }
    #endregion

    private (int start, int end) WinterberryStyles(ushort grassType)
    {
        if (grassType == (ushort)ModContent.TileType<SnowTaigaGrassTile>())
            return (6, 11);

        return (0, 5);
    }

    private static ushort GetPlantType(ushort grassType)
    {
        if (grassType == (ushort)ModContent.TileType<SnowTaigaGrassTile>())
            return (ushort)ModContent.TileType<SnowTaigaPlants>();

        if (grassType == (ushort)ModContent.TileType<CorruptTaigaGrassTile>())
            return (ushort)ModContent.TileType<CorruptTaigaPlants>();

        if (grassType == (ushort)ModContent.TileType<CrimsonTaigaGrassTile>())
            return (ushort)ModContent.TileType<CrimsonTaigaPlants>();

        return (ushort)ModContent.TileType<TaigaPlants>();
    }

    private bool HasSpacing(int x, int surfaceY, int minSpacing)
    {
        for (int checkX = x - minSpacing; checkX <= x + minSpacing; checkX++)
        {
            if (checkX == x) continue;
            if (!WorldGen.InWorld(checkX, surfaceY)) continue;

            if (IsTreeAt(checkX, surfaceY))
                return false;
        }

        return true;
    }

    private bool IsTreeAt(int x, int y)
    {
        for (int checkY = y - 10; checkY < y; checkY++)
        {
            if (!WorldGen.InWorld(x, checkY)) continue;

            var tile = Main.tile[x, checkY];
            if (tile.HasTile && (tile.TileType == ModContent.TileType<SpruceTree>() ||
                                tile.TileType == TileID.Trees))
            {
                return true;
            }
        }

        return false;
    }


    #region Helper Methods
    private ushort GetGrassType(int x, int y)
    {
        // Check for evil biomes first since they take priority
        if (HasNearbyEvil(x, y))
        {
            if (NearbyCorruption(x, y))
                return (ushort)ModContent.TileType<CorruptTaigaGrassTile>();
            if (NearbyCrimson(x, y))
                return (ushort)ModContent.TileType<CrimsonTaigaGrassTile>();
        }

        // Check for snow proximity
        if (NearSnow(x, y))
        {
            return (ushort)ModContent.TileType<SnowTaigaGrassTile>();
        }

        // Default taiga grass
        return (ushort)ModContent.TileType<TaigaGrassTile>();
    }

    private bool HasNearbyEvil(int x, int y)
    {
        return NearbyCorruption(x, y) || NearbyCrimson(x, y);
    }

    private bool NearbyCorruption(int x, int y)
    {
        for (var checkX = x - 30; checkX <= x + 30; checkX++)
        {
            for (var checkY = y - 30; checkY <= y + 30; checkY++)
            {
                if (!WorldGen.InWorld(checkX, checkY)) continue;

                var tile = Framing.GetTileSafely(checkX, checkY);

                if (tile.HasTile && IsCorruptionTile(tile.TileType))
                    return true;

                if (!tile.HasTile && IsCorruptionWall(tile.WallType))
                    return true;

            }
        }
        return false;
    }

    private bool NearbyCrimson(int x, int y)
    {
        for (var checkX = x - 30; checkX <= x + 30; checkX++)
        {
            for (var checkY = y - 30; checkY <= y + 30; checkY++)
            {
                if (!WorldGen.InWorld(checkX, checkY)) continue;

                var tile = Framing.GetTileSafely(checkX, checkY);

                if (tile.HasTile && IsCrimsonTile(tile.TileType))
                    return true;

                if (!tile.HasTile && IsCrimsonWall(tile.WallType))
                    return true;
            }
        }
        return false;
    }

    private bool NearSnow(int x, int y)
    {
        var distanceToSnowLeft = Math.Abs(x - GenVars.snowOriginLeft);
        var distanceToSnowRight = Math.Abs(x - GenVars.snowOriginRight);
        var closestSnowDistance = Math.Min(distanceToSnowLeft, distanceToSnowRight);

        if (closestSnowDistance <= 150)
            return true;

        for (var checkX = x - 30; checkX <= x + 30; checkX++)
        {
            for (var checkY = y - 30; checkY <= y + 30; checkY++)
            {
                if (!WorldGen.InWorld(checkX, checkY)) continue;

                var tile = Framing.GetTileSafely(checkX, checkY);
                if (tile.HasTile && IsSnowTile(tile.TileType))
                    return true;
            }
        }

        return false;
    }

    private bool IsCorruptionTile(ushort tileType)
    {
        return tileType == TileID.Ebonstone || tileType == TileID.Ebonsand ||
               tileType == TileID.CorruptGrass || tileType == TileID.CorruptSandstone ||
               tileType == TileID.CorruptHardenedSand || tileType == TileID.CorruptIce;
    }

    private bool IsCrimsonTile(ushort tileType)
    {
        return tileType == TileID.Crimstone || tileType == TileID.Crimsand ||
               tileType == TileID.CrimsonGrass || tileType == TileID.CrimsonSandstone ||
               tileType == TileID.CrimsonHardenedSand || tileType == TileID.FleshIce;
    }

    private bool IsSnowTile(ushort tileType)
    {
        return tileType == TileID.SnowBlock || tileType == TileID.IceBlock;
    }

    private bool IsCorruptionWall(ushort wallType)
    {
        return wallType >= WallID.CorruptionUnsafe1 && wallType <= WallID.CorruptionUnsafe4 ||
               wallType == WallID.EbonstoneUnsafe || wallType == WallID.CorruptHardenedSand ||
               wallType == WallID.CorruptGrassUnsafe;
    }

    private bool IsCrimsonWall(ushort wallType)
    {
        return wallType >= WallID.CrimsonUnsafe1 && wallType <= WallID.CrimsonUnsafe4 ||
               wallType == WallID.CrimstoneUnsafe || wallType == WallID.CrimsonHardenedSand ||
               wallType == WallID.CrimsonGrassUnsafe;
    }
    #endregion
}
