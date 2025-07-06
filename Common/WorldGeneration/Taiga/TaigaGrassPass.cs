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
            progress.Message = "No peat tiles found - skipping grass spread";
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

        tile.TileType = (ushort)ModContent.TileType<TaigaGrassTile>();
        WorldGen.SquareTileFrame(x, y, true);

        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
        }
    }
}
