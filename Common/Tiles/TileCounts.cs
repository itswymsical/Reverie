using Reverie.Content.Tiles.Canopy;
using Reverie.Content.Tiles.Taiga;

namespace Reverie.Common.Tiles;

internal class TileCounts : ModSystem
{
    public int canopyBlockCount;
    public int taigaCount;

    public static TileCounts Instance => ModContent.GetInstance<TileCounts>();

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        canopyBlockCount = tileCounts[ModContent.TileType<WoodgrassTile>()];
        taigaCount = tileCounts[ModContent.TileType<TaigaGrassTile>()];
    }
}
