using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria;
using Reverie.Utilities;
using Reverie.Content.Sylvanwalde.Tiles.Canopy;
using Reverie.Helpers;
using Terraria.ModLoader;
using Reverie.Content.Terraria.Tiles;

namespace Reverie.Common.Systems.WorldGeneration.GenPasses
{
    public class CavePass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating Caves";

            float totalProgress = 0f;
            float methodProgress = 0f;

            // DoCaveMap
            FastNoiseLite noise1 = new();
            noise1.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            noise1.SetFrequency(0.16f);
            noise1.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise1.SetFractalOctaves(4);
            noise1.SetFractalLacunarity(3f);
            noise1.SetFractalGain(0.27f);
            noise1.SetFractalWeightedStrength(0.12f);
            float killThreshold1 = 0.1f;
            float posx1 = Main.maxTilesX;
            float posy1 = Main.UnderworldLayer;

            float[,] noiseData1 = new float[(int)posx1, (int)posy1];
            for (float x = 0; x < posx1; x++)
            {
                for (float y = 0; y < posy1; y++)
                {
                    float worldX = x;
                    float worldY = y;
                    noiseData1[(int)x, (int)y] = noise1.GetNoise(worldX / 3, worldY - (worldY / 4));
                }
            }
            for (float x = 0; x < posx1; x++)
            {
                for (float y = (int)Main.rockLayer - (int)GenVars.worldSurfaceLow; y < posy1; y++)
                {
                    float worldX = x;
                    float worldY = y;
                    if (noiseData1[(int)x, (int)y] > killThreshold1)
                    {
                        Tile tile = Main.tile[(int)worldX, (int)worldY];
                        tile.HasTile = false;
                        progress.Set(((x * (posy1 - (GenVars.worldSurfaceLow + (GenVars.worldSurface / 2)))) + 
                            (y - (GenVars.worldSurfaceLow + (GenVars.worldSurface / 2)))) / 
                            (posx1 * (posy1 - (GenVars.worldSurfaceLow + (GenVars.worldSurface / 2)))) * 0.33f);
                    }
                }
            }
            methodProgress = .33f;
            totalProgress += methodProgress;
            progress.Set(totalProgress);

            // DoSmallHoles
            FastNoiseLite noise2 = new();
            noise2.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            noise2.SetFrequency(0.25f);
            noise2.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise2.SetFractalOctaves(4);
            noise2.SetFractalLacunarity(3f);
            noise2.SetFractalGain(0.27f);
            noise2.SetFractalWeightedStrength(0.12f);
            float killThreshold2 = 0.1f;
            float posx2 = Main.maxTilesX;
            float posy2 = Main.UnderworldLayer;
            float[,] noiseData2 = new float[(int)posx2, (int)posy2];
            for (float x = 0; x < posx2; x += 50)
            {
                for (float y = 0; y < posy2; y += 50)
                {
                    float worldX = x;
                    float worldY = y;
                    noiseData2[(int)x, (int)y] = noise2.GetNoise(worldX, worldY);
                }
            }
            for (float x = 0; x < posx2; x += 50)
            {
                for (float y = (float)GenVars.rockLayerHigh; y < posy2; y += 50)
                {
                    float worldX = x;
                    float worldY = y;
                    if (noiseData2[(int)x, (int)y] > killThreshold2)
                    {
                        Tile tile = Main.tile[(int)worldX, (int)worldY];
                        tile.HasTile = false;
                        progress.Set(((x / 50 * ((posy2 - GenVars.rockLayerHigh) / 50)) + ((y - GenVars.rockLayerHigh) / 50)) 
                            / ((posx2 / 50) * ((posy2 - GenVars.rockLayerHigh) / 50)) * 0.33f + 0.33f);

                    }
                }
            }

            // DoOpenCaves  
            FastNoiseLite noise3 = new();
            noise3.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise3.SetFrequency(0.048f);
            noise3.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise3.SetFractalOctaves(4);
            noise3.SetFractalLacunarity(3f);
            noise3.SetFractalGain(0.27f);
            noise3.SetFractalWeightedStrength(0.12f);
            float killThreshold3 = 0.1f;
            float posx3 = Main.maxTilesX;
            float posy3 = Main.UnderworldLayer;
            float[,] noiseData3 = new float[(int)posx3, (int)posy3];
            for (float x = 0; x < posx3; x++)
            {
                for (float y = 0; y < posy3; y++)
                {
                    float worldX = x;
                    float worldY = y;
                    noiseData3[(int)x, (int)y] = noise3.GetNoise(worldX, worldY);
                }
            }

            for (float x = 0; x < posx3; x++)
            {
                for (float y = (float)GenVars.rockLayerHigh; y < posy3; y++)
                {
                    float worldX = x;
                    float worldY = y;
                    if (noiseData3[(int)x, (int)y] > killThreshold3)
                    {
                        Tile tile = Main.tile[(int)worldX, (int)worldY];
                        tile.HasTile = false;
                        progress.Set((x * (posy3 - GenVars.rockLayerHigh) + (y - GenVars.rockLayerHigh)) / (posx3 * (posy3 - GenVars.rockLayerHigh)) * 0.34f + 0.66f);

                    }
                }
            }

            for (int x2 = (int)posx1 - (int)posx1; x2 <= posx1 + posx1; x2++)
            {
                for (int y2 = ((int)posy1 + 50) - (int)posy1; y2 <= (posy1 + 50) + posy1; y2++)
                {
                    if (Helper.GenerateCanopyShape(x2, y2, (int)posx1, (int)posy1, (int)posy1, (int)posy1, 0.04f, (int)posy1 / 3, 100, 15))
                    {
                        if (WorldGen.genRand.NextBool(180))
                            WorldGen.OreRunner(x2, y2, 5, 7, (ushort)ModContent.TileType<LodestoneTile>());
                    }
                }
            }
        }
    }
}
