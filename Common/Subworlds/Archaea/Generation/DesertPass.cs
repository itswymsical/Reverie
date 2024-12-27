using Reverie.Helpers;
using Reverie.Utilities;
using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;


namespace Reverie.Common.Systems.Subworlds.Archaea
{
    public class DesertPass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating Terrain";
            FastNoiseLite noise = new();
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(2);
            noise.SetFrequency(0.03f);

            FastNoiseLite caveNoise = new();
            caveNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            caveNoise.SetFrequency(0.05f);

            FastNoiseLite splotchNoise = new();
            splotchNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            splotchNoise.SetFrequency(0.02f);

            FastNoiseLite wallSplotchNoise = new();
            wallSplotchNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            wallSplotchNoise.SetFrequency(0.015f); // Slightly larger splotches for walls

            var surfaceY = Main.worldSurface = Main.maxTilesY / 5;
            Main.spawnTileX = Main.maxTilesX / 2;
            Main.spawnTileY = (int)surfaceY;
            int stoneLayerStart = (int)Main.rockLayer - ((int)Main.rockLayer / 6);

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                float noiseValue = noise.GetNoise(x / 2, Main.maxTilesY / 2);
                int peakHeight = (int)(Main.worldSurface - 10 + noiseValue * 25);
                peakHeight = Math.Clamp(peakHeight, 0, Main.maxTilesY - 1);
                bool isHardenedSandPatch = caveNoise.GetNoise(x, peakHeight) > 0.7f;

                for (int y = peakHeight; y < Main.UnderworldLayer; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = true;

                    float splotchValue = splotchNoise.GetNoise(x, y);
                    float wallSplotchValue = wallSplotchNoise.GetNoise(x, y);

                    // Determine base tile type
                    ushort baseTileType;
                    if (isHardenedSandPatch && y >= peakHeight && y < peakHeight + 60)
                        baseTileType = TileID.HardenedSand;
                    else if (y < peakHeight + 5)
                        baseTileType = TileID.Sand;
                    else if (y < stoneLayerStart)
                        baseTileType = TileID.HardenedSand;
                    else
                        baseTileType = TileID.Sandstone;

                    // Apply large splotches at layer transitions for tiles
                    int transitionRange = 20;

                    if (y >= peakHeight - transitionRange && y < peakHeight + transitionRange)
                    {
                        float transitionProgress = (float)(y - (peakHeight - transitionRange)) / (2 * transitionRange);
                        if (splotchValue > transitionProgress - 0.2f)
                            tile.TileType = TileID.Sand;
                        else
                            tile.TileType = TileID.HardenedSand;
                    }
                    else if (y >= stoneLayerStart - transitionRange && y < stoneLayerStart + transitionRange)
                    {
                        float transitionProgress = (float)(y - (stoneLayerStart - transitionRange)) / (2 * transitionRange);
                        if (splotchValue > transitionProgress - 0.2f)
                            tile.TileType = TileID.HardenedSand;
                        else
                            tile.TileType = TileID.Sandstone;
                    }
                    else
                    {
                        tile.TileType = baseTileType;

                        if (splotchValue > 0.7f)
                        {
                            if (baseTileType == TileID.Sand)
                                tile.TileType = TileID.HardenedSand;
                            else if (baseTileType == TileID.HardenedSand)
                                tile.TileType = Main.rand.NextBool() ? TileID.Sand : TileID.Sandstone;
                            else if (baseTileType == TileID.Sandstone)
                                tile.TileType = TileID.HardenedSand;
                        }
                    }

                    // Apply splotches to walls
                    if (y > peakHeight + 4)
                    {
                        if (y < stoneLayerStart - transitionRange)
                        {
                            tile.WallType = WallID.HardenedSandEcho;
                        }
                        else if (y >= stoneLayerStart - transitionRange && y < stoneLayerStart + transitionRange)
                        {
                            float transitionProgress = (float)(y - (stoneLayerStart - transitionRange)) / (2 * transitionRange);
                            if (wallSplotchValue > transitionProgress - 0.2f)
                                tile.WallType = WallID.HardenedSandEcho;
                            else
                                tile.WallType = WallID.SandstoneEcho;
                        }
                        else
                        {
                            tile.WallType = WallID.SandstoneEcho;
                        }

                        if (wallSplotchValue > 0.75f)
                        {
                            if (tile.WallType == WallID.HardenedSandEcho)
                                tile.WallType = WallID.SandstoneEcho;
                            else if (tile.WallType == WallID.SandstoneEcho)
                                tile.WallType = WallID.HardenedSandEcho;
                        }
                    }
                    else
                    {
                        tile.WallType = 0;
                    }
                }
            }
        }
    }
}