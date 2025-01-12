using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria;
using Reverie.Utilities;
using Reverie.Content.Terraria.Tiles;
using System;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;

namespace Reverie.Common.Systems.WorldGeneration.GenPasses
{
    public class CavePass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        private const int CHUNK_SIZE = 64;
        private const float CAVE_THRESHOLD = 0.1f;
        private const float OPEN_CAVE_THRESHOLD = 0.1f;
        private const float ORE_THRESHOLD = 0.1f;
        private const int ORE_STEP_SIZE = 58;

        private static readonly HashSet<ushort> ValidOreBlocks = new()
        {
            TileID.Stone,
            TileID.Dirt,
            TileID.Mud,
            TileID.Sandstone,
            TileID.HardenedSand,
            TileID.IceBlock,
            TileID.ClayBlock,
            TileID.Sand
        };

        private bool IsValidOreLocation(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && ValidOreBlocks.Contains(tile.TileType);
        }

        private FastNoiseLite SetupCaveNoise()
        {
            var noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            noise.SetFrequency(0.19f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(2);
            noise.SetFractalLacunarity(3f);
            noise.SetFractalGain(0.27f);
            noise.SetFractalWeightedStrength(0.12f);
            return noise;
        }

        private FastNoiseLite SetupOpenCaveNoise()
        {
            var noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFrequency(0.056f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(3);
            noise.SetFractalLacunarity(3f);
            noise.SetFractalGain(0.27f);
            noise.SetFractalWeightedStrength(0.12f);
            return noise;
        }

        private FastNoiseLite SetupMineralNoise()
        {
            var noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            noise.SetFrequency(0.12f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(3);
            noise.SetFractalLacunarity(3f);
            noise.SetFractalGain(0.27f);
            noise.SetFractalWeightedStrength(0.12f);
            return noise;
        }

        private void GenerateChunk(
            int startX,
            int startY,
            int width,
            int height,
            FastNoiseLite caveNoise,
            FastNoiseLite openCaveNoise,
            GenerationProgress progress)
        {
            int endX = Math.Min(startX + width, Main.maxTilesX);
            int endY = Math.Min(startY + height, Main.UnderworldLayer);
            float totalTiles = (float)(Main.maxTilesX * (Main.UnderworldLayer - (Main.rockLayer - GenVars.worldSurfaceLow)));

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    if (y < Main.rockLayer - GenVars.worldSurfaceLow)
                        continue;

                    float caveNoiseValue = caveNoise.GetNoise(x / 3f, y - (y / 4f));
                    float openCaveNoiseValue = openCaveNoise.GetNoise(x, y);

                    if (caveNoiseValue > CAVE_THRESHOLD ||
                        (y >= GenVars.rockLayerHigh && openCaveNoiseValue > OPEN_CAVE_THRESHOLD))
                    {
                        Tile tile = Main.tile[x, y];
                        tile.HasTile = false;
                    }

                    float currentProgress = (float)(((x * (Main.UnderworldLayer - (Main.rockLayer - GenVars.worldSurfaceLow))) +
                        (y - (Main.rockLayer - GenVars.worldSurfaceLow))) / totalTiles);
                    progress.Set(currentProgress);
                }
            }
        }

        private void GenerateOres()
        {
            var noise = SetupMineralNoise();

            // Surface ores (Copper/Tin)
            for (int x = 0; x < Main.maxTilesX; x += ORE_STEP_SIZE)
            {
                for (int y = (int)GenVars.worldSurfaceLow; y < Main.rockLayer; y += ORE_STEP_SIZE)
                {
                    float copperNoise = noise.GetNoise(x, y + 1000);
                    float tinNoise = noise.GetNoise(x, y + 2000);

                    if (copperNoise < ORE_THRESHOLD && IsValidOreLocation(x, y))
                        WorldGen.TileRunner(x, y, WorldGen.genRand.Next(7, 12), WorldGen.genRand.Next(7, 12), TileID.Copper);

                    if (tinNoise < ORE_THRESHOLD && IsValidOreLocation(x, y))
                        WorldGen.TileRunner(x, y, WorldGen.genRand.Next(7, 12), WorldGen.genRand.Next(7, 12), TileID.Tin);
                }
            }

            // Mid-level ores (Iron/Lead)
            for (int x = 0; x < Main.maxTilesX; x += ORE_STEP_SIZE)
            {
                for (int y = (int)(Main.rockLayer * 0.32); y < (int)Main.rockLayer; y += ORE_STEP_SIZE)
                {
                    float ironNoise = noise.GetNoise(x, y + 3000);
                    float leadNoise = noise.GetNoise(x, y + 4000);

                    if (ironNoise < ORE_THRESHOLD && IsValidOreLocation(x, y))
                        WorldGen.TileRunner(x, y, WorldGen.genRand.Next(7, 11), WorldGen.genRand.Next(7, 11), TileID.Iron);

                    if (leadNoise < ORE_THRESHOLD && IsValidOreLocation(x, y))
                        WorldGen.TileRunner(x, y, WorldGen.genRand.Next(7, 11), WorldGen.genRand.Next(7, 11), TileID.Lead);
                }
            }

            // Deep ores (Silver/Tungsten, Gold/Platinum)
            for (int x = 0; x < Main.maxTilesX; x += ORE_STEP_SIZE)
            {
                for (int y = (int)GenVars.rockLayerHigh; y < Main.maxTilesY * 0.8f; y += ORE_STEP_SIZE)
                {
                    if (y < Main.maxTilesY * 0.6f)  // Silver/Tungsten layer
                    {
                        float silverNoise = noise.GetNoise(x, y + 5000);
                        float tungstenNoise = noise.GetNoise(x, y + 6000);

                        if (silverNoise < ORE_THRESHOLD && IsValidOreLocation(x, y))
                            WorldGen.TileRunner(x, y, WorldGen.genRand.Next(5, 9), WorldGen.genRand.Next(5, 9), TileID.Silver);

                        if (tungstenNoise < ORE_THRESHOLD && IsValidOreLocation(x, y))
                            WorldGen.TileRunner(x, y, WorldGen.genRand.Next(5, 9), WorldGen.genRand.Next(5, 9), TileID.Tungsten);
                    }
                    else  // Gold/Platinum layer
                    {
                        float goldNoise = noise.GetNoise(x, y + 7000);
                        float platinumNoise = noise.GetNoise(x, y + 8000);

                        if (goldNoise < ORE_THRESHOLD && IsValidOreLocation(x, y))
                            WorldGen.TileRunner(x, y, WorldGen.genRand.Next(5, 7), WorldGen.genRand.Next(5, 7), TileID.Gold);

                        if (platinumNoise < ORE_THRESHOLD && IsValidOreLocation(x, y))
                            WorldGen.TileRunner(x, y, WorldGen.genRand.Next(4, 7), WorldGen.genRand.Next(4, 7), TileID.Platinum);
                    }
                }
            }
        }

        private void GenerateLodestone()
        {
            var noise = SetupMineralNoise();
            const float LODESTONE_THRESHOLD = 0.1f;

            for (int x = 0; x < Main.maxTilesX; x += ORE_STEP_SIZE)
            {
                for (int y = (int)GenVars.rockLayerHigh; y < Main.UnderworldLayer; y += ORE_STEP_SIZE)
                {
                    float noiseValue = noise.GetNoise(x, y + 9000);  // Different offset for Lodestone

                    if (noiseValue < LODESTONE_THRESHOLD && IsValidOreLocation(x, y))
                    {
                        WorldGen.TileRunner(
                            x, y,
                            WorldGen.genRand.Next(3, 6),
                            WorldGen.genRand.Next(3, 6),
                            (ushort)ModContent.TileType<LodestoneTile>()
                        );
                    }
                }
            }
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating Cave Networks";

            var caveNoise = SetupCaveNoise();
            var openCaveNoise = SetupOpenCaveNoise();

            for (int y = 0; y < Main.UnderworldLayer; y += CHUNK_SIZE)
            {
                for (int x = 0; x < Main.maxTilesX; x += CHUNK_SIZE)
                {
                    GenerateChunk(x, y, CHUNK_SIZE, CHUNK_SIZE, caveNoise, openCaveNoise, progress);
                }
            }

            GenerateOres();
            GenerateLodestone();
        }
    }
}