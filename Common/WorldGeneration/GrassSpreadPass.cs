using System.Collections.Generic;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;

public abstract class GrassSpreadPass : GenPass
{
    protected GrassSpreadPass(string name, float weight) : base(name, weight) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = GetProgressMessage();
        SpreadGrass(progress);
    }

    private void SpreadGrass(GenerationProgress progress)
    {
        var soilTiles = FindSoil();
        if (soilTiles.Count == 0)
            return;

        var processed = 0;
        foreach (var soilPos in soilTiles)
        {
            processed++;
            if (processed % 100 == 0)
                progress.Set((double)processed / soilTiles.Count);

            SpreadGrassAt(soilPos.X, soilPos.Y);
        }
    }

    private List<Point> FindSoil()
    {
        var soilTiles = new List<Point>();
        var soilType = GetSoilTileType();

        for (var x = 100; x < Main.maxTilesX - 100; x++)
        {
            for (var y = 50; y < Main.maxTilesY - 100; y++)
            {
                if (!WorldGen.InWorld(x, y)) continue;

                var tile = Main.tile[x, y];
                if (tile.HasTile && tile.TileType == soilType)
                {
                    soilTiles.Add(new Point(x, y));
                }
            }
        }

        return soilTiles;
    }

    private void SpreadGrassAt(int x, int y)
    {
        var tile = Framing.GetTileSafely(x, y);
        if (!tile.HasTile || tile.TileType != GetSoilTileType())
            return;

        if (IsExposedToAir(x, y) && CanSpreadAt(x, y))
        {
            ConvertToGrass(x, y);
        }
    }

    protected virtual bool IsExposedToAir(int x, int y)
    {
        for (var checkX = x - 1; checkX <= x + 1; checkX++)
        {
            for (var checkY = y - 1; checkY <= y + 1; checkY++)
            {
                if (checkX == x && checkY == y) continue;
                if (!WorldGen.InWorld(checkX, checkY)) continue;

                var neighborTile = Framing.GetTileSafely(checkX, checkY);
                if (!neighborTile.HasTile)
                    return true;
            }
        }
        return false;
    }

    private void ConvertToGrass(int x, int y)
    {
        var tile = Framing.GetTileSafely(x, y);
        if (!tile.HasTile) return;

        tile.TileType = GetGrassTileType();
        WorldGen.SquareTileFrame(x, y, true);

        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
        }

        OnGrassConverted(x, y);
    }

    #region Abstract Methods
    protected abstract string GetProgressMessage();
    protected abstract ushort GetSoilTileType(); // Soil tile that gets converted to grass
    protected abstract ushort GetGrassTileType(); // Grass tile result
    #endregion

    #region Virtual Methods
    protected virtual bool CanSpreadAt(int x, int y) => true;
    protected virtual void OnGrassConverted(int x, int y) { }
    #endregion
}
