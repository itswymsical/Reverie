using Reverie.Content.Tiles.Taiga;

namespace Reverie.Common.Tiles;

internal class TileCounts : ModSystem
{
    public int undergroundCanopyBlockCount;
    public int surfaceCanopyBlockCount;
    public int taigaCount;

    public static TileCounts Instance => ModContent.GetInstance<TileCounts>();

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {

        taigaCount = tileCounts[ModContent.TileType<TaigaGrassTile>()];
    }
}
