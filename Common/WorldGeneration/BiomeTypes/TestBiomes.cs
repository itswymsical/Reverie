using System;

namespace Reverie.Common.WorldGeneration.BiomeTypes;

public class ExampleTemperateBiome : FilterBiome
{
    public ExampleTemperateBiome() : base("[Example] Temperate", 249f) { }

    protected override string PassName => "Temperate Forest";

    protected override ushort GetBaseTileType(int x, int y, int depthFromSurface, float noiseValue)
    {
        // For conversion biomes, we don't replace in base patch
        return 0;
    }

    protected override ushort GetTerrainTileType(int x, int y, int depthFromSurface)
    {
        // For conversion biomes, terrain generation is handled in PopulateBiome
        return 0;
    }

    protected override void ConvertTilesInColumn(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return;

        Tile tile = Main.tile[x, y];
        if (!tile.HasTile) return;

        // Convert existing tiles to temperate variants
        switch (tile.TileType)
        {
            case TileID.Grass:
            case TileID.CrimsonGrass:
            case TileID.CorruptGrass:
                // tile.TileType = (ushort)ModContent.TileType<TemperateGrassTile>();
                break;
            case TileID.Plants:
            case TileID.Plants2:
                bool hasVariantFrame = tile.TileFrameX > 17;
                // tile.TileType = (ushort)ModContent.TileType<TemperatePlants>();
                if (hasVariantFrame)
                    tile.TileFrameX = (short)(WorldGen.genRand.Next(18) * 18);
                break;
            case TileID.Saplings:
                // tile.TileType = (ushort)ModContent.TileType<BirchSapling>();
                break;
        }
    }
}
