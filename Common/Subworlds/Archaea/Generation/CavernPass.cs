using Reverie.Utilities;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Reverie.Common.Systems.Subworlds.Archaea
{
    public class CavernPass : GenPass
    {
        public CavernPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating Caves";

            float totalProgress = 0f;
            float methodProgress = 0f;

            // DoCaveMap
            FastNoiseLite noise1 = new();
            noise1.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            noise1.SetFrequency(0.21f);
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
                for (float y = (int)Main.worldSurface + 28; y < posy1; y++)
                {
                    float worldX = x;
                    float worldY = y;
                    if (noiseData1[(int)x, (int)y] > killThreshold1)
                    {
                        Tile tile = Main.tile[(int)worldX, (int)worldY];
                        tile.HasTile = false;
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
                for (float y = (float)Main.rockLayer; y < posy2; y += 50)
                {
                    float worldX = x;
                    float worldY = y;
                    if (noiseData2[(int)x, (int)y] > killThreshold2)
                    {
                        Tile tile = Main.tile[(int)worldX, (int)worldY];
                        tile.HasTile = false;
                    }
                }
            }
            methodProgress = .33f;
            totalProgress += methodProgress;
            progress.Set(totalProgress);

            // DoOpenCaves  
            FastNoiseLite noise3 = new();
            noise3.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise3.SetFrequency(0.033f);
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
                for (float y = (float)Main.rockLayer; y < posy3; y++)
                {
                    float worldX = x;
                    float worldY = y;
                    if (noiseData3[(int)x, (int)y] > killThreshold3)
                    {
                        Tile tile = Main.tile[(int)worldX, (int)worldY];
                        tile.HasTile = false;
                    }
                }
            }
            methodProgress = .34f;
            totalProgress += methodProgress;
            progress.Set(totalProgress);

        }
    }
}