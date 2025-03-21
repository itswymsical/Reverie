using Reverie.Content.Tiles.Archaea;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Archaea.Generation;

public class RubblePass : GenPass
{
    private const int MINIMUM_EMPTY_SPACE = 2;
    private const float PLACEMENT_CHANCE = 0.15f;

    public RubblePass() : base("Primordial Rubble", 77.43f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Scattering ancient rubble...";

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = 10; y < Main.worldSurface + 10; y++)
            {
                if (WorldGen.genRand.NextFloat() > PLACEMENT_CHANCE)
                    continue;

                TryPlaceRubble(x, y);
            }
        }
    }

    #region Helpers
    private bool TryPlaceRubble(int x, int y)
    {
        Tile tile = Main.tile[x, y];

        if (!tile.HasTile || tile.TileType != ModContent.TileType<PrimordialSandTile>())
            return false;

        if (!HasRequiredClearance(x, y))
            return false;

        return WorldGen.genRand.Next(3) switch
        {
            0 => PlaceSmallRubble(x, y),
            1 => PlaceMediumRubble(x, y),
            _ => PlaceLargeRubble(x, y)
        };
    }

    private bool HasRequiredClearance(int x, int y)
    {
        for (int i = 1; i <= MINIMUM_EMPTY_SPACE; i++)
        {
            if (y - i < 10) return false;
            if (Main.tile[x, y - i].HasTile) return false;
        }
        return true;
    }

    private bool PlaceSmallRubble(int x, int y)
    {
        WorldGen.PlaceTile(x, y - 1,
            ModContent.TileType<PrimordialRubble1x1Natural>(),
            style: WorldGen.genRand.Next(6));
        return true;
    }

    private bool PlaceMediumRubble(int x, int y)
    {
        if (x + 1 >= Main.maxTilesX) return false;
        if (Main.tile[x + 1, y].HasTile &&
            Main.tile[x + 1, y].TileType != ModContent.TileType<PrimordialSandTile>())
            return false;

        WorldGen.PlaceTile(x, y - 1,
            ModContent.TileType<PrimordialRubble2x1Natural>(),
            style: WorldGen.genRand.Next(6));
        return true;
    }

    private bool PlaceLargeRubble(int x, int y)
    {
        if (x + 2 >= Main.maxTilesX) return false;

        for (int i = 0; i < 3; i++)
        {
            if (!Main.tile[x + i, y].HasTile ||
                Main.tile[x + i, y].TileType != ModContent.TileType<PrimordialSandTile>())
                return false;
        }

        WorldGen.PlaceTile(x, y - 2,
            ModContent.TileType<PrimordialRubble3x2Natural>(),
            style: WorldGen.genRand.Next(6));
        return true;
    }
    #endregion
}
