using Reverie.Content.Tiles.Taiga;
using Reverie.lib;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TaigaPlantPass : GenPass
{
    public TaigaPlantPass() : base("Spreading Tundra Grass", 247.5f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing Taiga Grass";

        int snowWidthPercent = WorldGen.genRand.Next(44, 57);
        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;
        int tundraWidth = rightEdge - leftEdge;
        int snowCoreStart = leftEdge + (tundraWidth * (100 - snowWidthPercent) / 200);
        int snowCoreEnd = rightEdge - (tundraWidth * (100 - snowWidthPercent) / 200);

        PlaceDecorations(leftEdge, rightEdge, snowCoreStart, snowCoreEnd, progress);
    }
    
    private static void PlaceDecorations(int left, int right, int snowCoreStart, int snowCoreEnd, GenerationProgress progress)
    {
        progress.Message = "Growing Plants";

        var noise = new FastNoiseLite(WorldGen.genRand.Next());
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.04f);

        var top = (int)(Main.worldSurface * 0.35);
        var bottom = (int)(Main.worldSurface * 1.2);

        var total = right - left;
        var current = 0;

        for (var x = left; x < right; x++)
        {
            progress.Set((float)current++ / total);

            var surfaceY = FindSurface(x, top, bottom);
            if (surfaceY <= 0) continue;

            var tile = Main.tile[x, surfaceY];
            var tileAbove = Main.tile[x, surfaceY - 1];

            if (tile.HasTile && (tile.TileType == (ushort)ModContent.TileType<TaigaGrassTile>() || tile.TileType == (ushort)ModContent.TileType<SnowTaigaGrassTile>()) && !tileAbove.HasTile)
            {
                var noiseValue = noise.GetNoise(x * 0.1f, surfaceY * 0.1f) * 0.5f + 0.5f;

                if (WorldGen.genRand.NextFloat() < 0.4f * noiseValue)
                {
                    if (x >= snowCoreStart && x <= snowCoreEnd)
                    {
                        PlaceSapling(x, surfaceY - 1);
                    }
                    else
                    {
                        PlacePlant(x, surfaceY - 1);
                    }
                }
            }
        }
    }

    private static int FindSurface(int x, int startY, int endY)
    {
        for (var y = startY; y < endY; y++)
        {
            if (Main.tile[x, y].HasTile && Main.tile[x, y].BlockType == BlockType.Solid)
            {
                return y;
            }
        }
        return -1;
    }

    private static void PlacePlant(int x, int y)
    {
        WorldGen.PlaceTile(x, y, (ushort)ModContent.TileType<SnowTaigaPlants>(), style: 0, mute: true);

        var plantTile = Main.tile[x, y];
        if (plantTile.HasTile)
        {
            plantTile.TileFrameY = 0;
            plantTile.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);

            WorldGen.SquareTileFrame(x, y, true);
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
            }
        }
    }

    private static void PlaceSapling(int x, int y)
    {
        WorldGen.PlaceTile(x, y, TileID.Saplings, style: 0, mute: true);
        WorldGen.GrowTree(x, y);
    }
}