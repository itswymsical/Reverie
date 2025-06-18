using Reverie.Content.Tiles.Rainforest;
using Reverie.Content.Tiles.Rainforest.Surface;
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
        surfaceCanopyBlockCount = tileCounts[ModContent.TileType<RainforestGrassTile>()];
        surfaceCanopyBlockCount = tileCounts[ModContent.TileType<OxisolTile>()];

        undergroundCanopyBlockCount = tileCounts[ModContent.TileType<WoodgrassTile>()];

        taigaCount = tileCounts[ModContent.TileType<TaigaGrassTile>()];
    }
}
