using Reverie.Content.Tiles.Taiga;
using System.Collections.Generic;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TaigaGrassPass : GenPass
{
    public TaigaGrassPass() : base("[Reverie] Taiga Grass", 248f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Spreading taiga grass...";
        SpreadTaigaGrass(progress);
    }

    private void SpreadTaigaGrass(GenerationProgress progress)
    {
        var peatTiles = FindPeatTiles();
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

    private List<Point> FindPeatTiles()
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
            ConvertToGrass(x, y);
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

    private void ConvertToGrass(int x, int y)
    {
        var tile = Framing.GetTileSafely(x, y);
        if (!tile.HasTile) return;

        var grassType = GetAppropriateGrassType(x, y);
        tile.TileType = grassType;
        if (Main.rand.NextBool(2))
        {
            WorldGen.PlaceTile(x, y - 1, GetPlantTypeForGrass(grassType), style: Main.rand.Next(18));
        }

        WorldGen.SquareTileFrame(x, y, true);
        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
        }
    }
    private static ushort GetPlantTypeForGrass(ushort grassType)
    {
        if (grassType == (ushort)ModContent.TileType<SnowTaigaGrassTile>())
            return (ushort)ModContent.TileType<SnowTaigaPlants>();

        if (grassType == (ushort)ModContent.TileType<CorruptTaigaGrassTile>())
            return (ushort)ModContent.TileType<CorruptTaigaPlants>();

        if (grassType == (ushort)ModContent.TileType<CrimsonTaigaGrassTile>())
            return (ushort)ModContent.TileType<CrimsonTaigaPlants>();

        return (ushort)ModContent.TileType<TaigaPlants>();
    }
    private ushort GetAppropriateGrassType(int x, int y)
    {
        // Check for evil biomes first since they take priority
        if (HasNearbyEvil(x, y))
        {
            if (HasNearbyCorruption(x, y))
                return (ushort)ModContent.TileType<CorruptTaigaGrassTile>();
            if (HasNearbyCrimson(x, y))
                return (ushort)ModContent.TileType<CrimsonTaigaGrassTile>();
        }

        // Check for snow proximity
        if (IsNearSnowBiome(x, y))
        {
            return (ushort)ModContent.TileType<SnowTaigaGrassTile>();
        }

        // Default taiga grass
        return (ushort)ModContent.TileType<TaigaGrassTile>();
    }

    private bool HasNearbyEvil(int x, int y)
    {
        return HasNearbyCorruption(x, y) || HasNearbyCrimson(x, y);
    }

    private bool HasNearbyCorruption(int x, int y)
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

    private bool HasNearbyCrimson(int x, int y)
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

    private bool IsNearSnowBiome(int x, int y)
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
}
