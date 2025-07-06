using Reverie.Content.Tiles;
using Reverie.Content.Tiles.Misc;
using Reverie.Content.Tiles.TemperateForest;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;
public class DecorPass : GenPass
{
    public DecorPass() : base("[Reverie] Biome Decor", 30f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Placing Decorations";

        for (int x = 100; x < Main.maxTilesX - 100; x++)
        {
            for (int y = (int)Main.worldSurface - 300; y < (int)Main.worldSurface; y++)
            {
                //if (WorldGen.genRand.NextBool(200))
                //{
                //    AddWoodLog(x, y);
                //}
                if (WorldGen.genRand.NextBool(140))
                {
                    AddMagnoliaFlower(x, y);
                }
                if (WorldGen.genRand.NextBool(30))
                {
                    AddDeadBush(x, y);
                }
            }
        }
    }

    private void AddWoodLog(int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);
        Tile tileAbove = Framing.GetTileSafely(x, y - 1);

        if (!tile.HasTile) return;

        bool validTile = tile.TileType == TileID.Grass;

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

    private void AddMagnoliaFlower(int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);
        Tile tileAbove = Framing.GetTileSafely(x, y - 1);
        if (!tile.HasTile) return;
        bool validTile = tile.TileType == TileID.Grass;
        if (!validTile) return;
        if (validTile && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
        {
            if (HasFlowerSpace(x, y))
            {
                WorldGen.PlaceTile(x, y - 1, ModContent.TileType<MagnoliaTile>(), mute: true, style: 2);
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, x, y - 1, 1, TileChangeType.None);
                }
            }
        }
    }

    private bool HasFlowerSpace(int x, int y)
    {
        if (!WorldGen.InWorld(x, y - 1)) return false;
        Tile checkTile = Framing.GetTileSafely(x, y - 1);

        return !checkTile.HasTile || checkTile.TileType == TileID.Plants2;
    }

    private bool HasSpace(int x, int y, int width, int height)
    {
        for (int checkX = x; checkX < x + width; checkX++)
        {
            for (int checkY = y - height; checkY < y; checkY++)
            {
                if (!WorldGen.InWorld(checkX, checkY)) return false;

                Tile checkTile = Framing.GetTileSafely(checkX, checkY);
                if (checkTile.HasTile && checkTile.TileType != TileID.Plants2)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void AddDeadBush(int x, int y)
    {
        if (!IsNotABeach(x)) return;

        Tile tile = Framing.GetTileSafely(x, y);
        Tile tileAbove = Framing.GetTileSafely(x, y - 1);
        if (!tile.HasTile) return;

        bool validTile = tile.TileType == TileID.Sand || tile.TileType == TileID.HardenedSand;
        if (!validTile) return;

        if (validTile && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
        {
            if (HasFlowerSpace(x, y))
            {
                WorldGen.PlaceTile(x, y - 1, ModContent.TileType<DeadbushTile>(), mute: true, style: Main.rand.Next(2));
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, x, y - 1, 1, TileChangeType.None);
                }
            }
        }
    }
   private bool IsNotABeach(int x)
    {
        return x > Main.maxTilesX * 0.15f && x < Main.maxTilesX - (Main.maxTilesX * 0.15f);
    }

    private void ClearGrassBlades(int x, int y)
    {
        for (int removeX = x; removeX < x + 3; removeX++)
        {
            for (int removeY = y - 2; removeY < y; removeY++)
            {
                if (!WorldGen.InWorld(removeX, removeY)) continue;

                Tile tile = Framing.GetTileSafely(removeX, removeY);
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
