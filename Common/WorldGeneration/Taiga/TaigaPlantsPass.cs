﻿using Terraria.IO;
using Terraria.WorldBuilding;
using Reverie.Content.Tiles.Taiga;
using Reverie.lib;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TaigaPlantPass : GenPass
{
    public TaigaPlantPass() : base("Spreading Tundra Grass", 247.5f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing Taiga Grass";

        // Calculate taiga boundaries using the same logic as TaigaPass
        int leftEdge = GenVars.snowOriginLeft;
        int rightEdge = GenVars.snowOriginRight;
        int snowWidth = rightEdge - leftEdge;
        int worldCenter = Main.maxTilesX / 2;
        int taigaWidth = snowWidth * 2;

        // Determine if snow biome is more to the left or right of world center
        int snowCenter = leftEdge + (snowWidth / 2);
        bool placeOnLeft = snowCenter > worldCenter;

        // Calculate taiga boundaries based on position
        int taigaLeft, taigaRight;
        if (placeOnLeft)
        {
            // Place taiga to the left of snow
            taigaRight = leftEdge;
            taigaLeft = taigaRight - taigaWidth;
        }
        else
        {
            // Place taiga to the right of snow
            taigaLeft = rightEdge;
            taigaRight = taigaLeft + taigaWidth;
        }

        // Ensure we don't go outside world bounds
        taigaLeft = Math.Max(0, taigaLeft);
        taigaRight = Math.Min(Main.maxTilesX - 1, taigaRight);

        PlaceDecorations(taigaLeft, taigaRight, progress);
    }

    private static void PlaceDecorations(int left, int right, GenerationProgress progress)
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
                    PlacePlant(x, surfaceY - 1);
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
}
