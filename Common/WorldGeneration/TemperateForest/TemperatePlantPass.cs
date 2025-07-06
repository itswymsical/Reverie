using Reverie.Content.Tiles.TemperateForest;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.TemperateForest;

public class TemperatePlantPass : GenPass
{
    public TemperatePlantPass() : base("[Reverie] Temperate Decor", 30f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Placing Forest Decorations";

        for (var x = 100; x < Main.maxTilesX - 100; x++)
        {
            for (var y = (int)Main.worldSurface - 300; y < (int)Main.worldSurface; y++)
            {
                if (WorldGen.genRand.NextBool(90))
                {
                    AddWoodLog(x, y);
                }
                if (WorldGen.genRand.NextBool(30))
                {
                    AddGrassBlades(x, y);
                }
            }
        }
    }

    private void AddWoodLog(int x, int y)
    {
        var tile = Framing.GetTileSafely(x, y);
        var tileAbove = Framing.GetTileSafely(x, y - 1);

        if (!tile.HasTile) return;

        var validTile = tile.TileType == ModContent.TileType<TemperateGrassTile>();

        if (!validTile) return;

        if (validTile && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
        {
            ClearGrassBlades(x, y);

            if (HasSpace(x, y, 3, 2))
            {
                WorldGen.PlaceTile(x, y - 1, ModContent.TileType<WoodLogTile>(), mute: true);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, x, y - 1, 3, TileChangeType.None);
                }
            }
        }
    }

    private void AddTemperateRock(int x, int y)
    {
        var tile = Framing.GetTileSafely(x, y);
        var tileAbove = Framing.GetTileSafely(x, y - 1);

        if (!tile.HasTile) return;

        var validTile = tile.TileType == ModContent.TileType<TemperateGrassTile>();

        if (!validTile) return;

        if (validTile && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
        {
            ClearGrassBlades(x, y);

            if (HasSpace(x, y, 3, 2))
            {
                WorldGen.PlaceTile(x, y - 1, 185, mute: true);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, x, y - 1, 3, TileChangeType.None);
                }
            }
        }
    }

    private void AddGrassBlades(int x, int y)
    {
        var tile = Framing.GetTileSafely(x, y);
        var tileAbove = Framing.GetTileSafely(x, y - 1);
        if (!tile.HasTile) return;
        var validTile = tile.TileType == ModContent.TileType<TemperateGrassTile>();
        if (!validTile) return;
        if (validTile && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
        {
            if (HasRoom(x, y))
            {
                WorldGen.PlaceTile(x, y - 1, ModContent.TileType<TemperatePlants>(), mute: true);
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, x, y - 1, 1, TileChangeType.None);
                }
            }
        }
    }

    private bool HasRoom(int x, int y)
    {
        if (!WorldGen.InWorld(x, y - 1)) return false;
        var checkTile = Framing.GetTileSafely(x, y - 1);

        return !checkTile.HasTile || checkTile.TileType == TileID.Plants2;
    }

    private bool HasSpace(int x, int y, int width, int height)
    {
        for (var checkX = x; checkX < x + width; checkX++)
        {
            for (var checkY = y - height; checkY < y; checkY++)
            {
                if (!WorldGen.InWorld(checkX, checkY)) return false;

                var checkTile = Framing.GetTileSafely(checkX, checkY);
                if (checkTile.HasTile && checkTile.TileType != TileID.Plants2)
                {
                    return false;
                }
            }
        }
        return true;
    }
    private bool IsNotABeach(int x)
    {
        return x > Main.maxTilesX * 0.15f && x < Main.maxTilesX - Main.maxTilesX * 0.15f;
    }

    private void ClearGrassBlades(int x, int y)
    {
        for (var removeX = x; removeX < x + 3; removeX++)
        {
            for (var removeY = y - 2; removeY < y; removeY++)
            {
                if (!WorldGen.InWorld(removeX, removeY)) continue;

                var tile = Framing.GetTileSafely(removeX, removeY);
                if (tile.HasTile && tile.TileType == TileID.Plants2)
                {
                    WorldGen.KillTile(removeX, removeY, false, false, false);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendTileSquare(-1, removeX, removeY, 1, TileChangeType.None);
                    }
                }
            }
        }
    }
}