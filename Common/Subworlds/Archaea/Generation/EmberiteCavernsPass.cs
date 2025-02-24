using Reverie.Utilities;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Archaea.Generation;

//public class EmberiteCavernsPass(string name, float loadWeight) : GenPass(name, loadWeight)
//{

//    public static int nestH = (int)(Main.maxTilesX * 0.065f);
//    public static int nestV = (int)(Main.maxTilesY * 0.175f);

//    private int crystalsPlaced = 0;
//    private readonly int[] validGroundTypes = [ModContent.TileType<Carbon>()];

//    private const float CrystalCoverage = 0.65f;
//    private const int AverageCrystalWidth = 4;
//    private const int AverageCrystalHeight = 4;
//    private const float CrystalSpacingFactor = 1.5f;

//    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
//    {
//        progress.Message = "Generating Emberite Nests";

//        int numBiomes = Main.rand.Next(3, 6);
//        for (int i = 0; i < numBiomes; i++)
//        {
//            int nestX = Main.rand.Next(nestH, Main.maxTilesX - nestH);
//            int nestY = Main.rand.Next((int)Main.rockLayer, (int)Main.UnderworldLayer - nestV);

//            // Check if the new biome is too close to any existing biomes
//            bool tooClose = false;
//            for (int j = 0; j < i; j++)
//            {
//                int otherNestX = Main.rand.Next(nestH, Main.maxTilesX - nestH);
//                int otherNestY = Main.rand.Next((int)Main.rockLayer, (int)Main.UnderworldLayer - nestV);
//                if (Math.Abs(nestX - otherNestX) < nestH * 2 && Math.Abs(nestY - otherNestY) < nestV * 2)
//                {
//                    tooClose = true;
//                    break;
//                }
//            }

//            if (!tooClose)
//            {
//                GenerateBiome(nestX, nestY);
//            }
//            else
//            {
//                i--; // Retry this iteration if the biome was too close
//            }
//        }
//    }

//    private void GenerateBiome(int nestX, int nestY)
//    {
//        for (int x = nestX - nestH; x <= nestX + nestH; x++)
//        {
//            for (int y = nestY - nestV; y <= nestY + nestV; y++)
//            {
//                if (Helper.VanillaMountain(x, y, nestX, nestY, nestH, nestV))
//                {
//                    Tile tile = Main.tile[x, y];
//                    WorldGen.KillWall(x, y);
//                    tile.TileType = (ushort)ModContent.TileType<Carbon>();
//                    tile.HasTile = true;
//                }
//            }
//        }
//        DoCaves(nestX, nestY);
//        //PlaceLava(nestX, nestY);
//        PlaceEmberiteCrystals(nestX, nestY);
//    }

//    private void DoCaves(int nestX, int nestY)
//    {
//        FastNoiseLite roots = new FastNoiseLite(Main.ActiveWorldFileData.Seed);
//        roots.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
//        roots.SetFractalType(FastNoiseLite.FractalType.FBm);
//        roots.SetFrequency(0.033f);
//        roots.SetFractalGain(0.435f);
//        roots.SetFractalOctaves(4);
//        roots.SetFractalLacunarity(3f);
//        roots.SetFractalGain(0.27f);
//        roots.SetFractalWeightedStrength(0.12f);

//        int posx = (nestH - 30) * 2;
//        int posy = (nestV - 30) * 2;
//        float threshold = 0.3f;

//        float[,] noiseData = new float[posx, posy];

//        for (int x = 0; x < posx; x++)
//        {
//            for (int y = 0; y < posy; y++)
//            {
//                int worldX = x + (nestX - nestH);
//                int worldY = y + (nestY - nestV);

//                noiseData[x, y] = roots.GetNoise(worldX / 2, worldY);
//            }
//        }

//        for (int x = 0; x < posx; x++)
//        {
//            for (int y = 0; y < posy; y++)
//            {
//                int worldX = x + (nestX - nestH);
//                int worldY = y + (nestY - nestV);
//                if (Helper.VanillaMountain(worldX, worldY, nestX, nestY, nestH, nestV))
//                {
//                    if (noiseData[x, y] > threshold)
//                        WorldGen.KillTile(worldX, worldY);

//                    if (noiseData[x, y] < threshold * 0.2f)
//                        WorldGen.KillTile(worldX, worldY);
//                }
//            }
//        }
//    }

//    private void PlaceLava(int nestX, int nestY)
//    {
//        for (int x = nestX - nestH; x <= nestX + nestH; x++)
//        {
//            for (int y = nestY - nestV; y <= nestY + nestV; y++)
//            {
//                if (Helper.VanillaMountain(x, y, nestX, nestY, nestH, nestV))
//                {
//                    if (Main.rand.NextBool(2))
//                    {
//                        WorldGen.PlaceLiquid(x - 1, y, (byte)LiquidID.Lava, 225);
//                        WorldGen.PlaceLiquid(x, y, (byte)LiquidID.Lava, 225);
//                        WorldGen.PlaceLiquid(x + 1, y, (byte)LiquidID.Lava, 225);
//                    }
//                }
//            }
//        }
//    }

//    private void PlaceEmberiteCrystals(int nestX, int nestY)
//    {
//        int biomeArea = (nestH * 2) * (nestV * 2);
//        int averageCrystalSize = AverageCrystalWidth * AverageCrystalHeight;
//        int maxCrystals = (int)((biomeArea * CrystalCoverage) / (averageCrystalSize * CrystalSpacingFactor));

//        for (int i = 0; i < maxCrystals; i++)
//        {
//            int x = nestX + Main.rand.Next(-nestH, nestH);
//            int y = nestY + Main.rand.Next(-nestV, nestV);

//            if (TryPlaceCrystal(x, y))
//            {
//                crystalsPlaced++;
//            }
//        }
//    }

//    private bool TryPlaceCrystal(int x, int y)
//    {
//        const int maxAttempts = 50;
//        int attempts = 0;

//        while (attempts < maxAttempts)
//        {
//            // Find the ground level
//            while (y < Main.maxTilesY - 10 && !Main.tile[x, y].HasTile)
//            {
//                y++;
//            }

//            // Check if we have a valid placement location
//            if (IsValidPlacement(x, y))
//            {
//                PlaceRandomCrystal(x, y - 1);
//                return true;
//            }

//            // Move to a nearby location and try again
//            x += Main.rand.Next(-5, 6);
//            y += Main.rand.Next(-3, 4);
//            attempts++;
//        }

//        return false;
//    }

//    private bool IsValidPlacement(int x, int y)
//    {
//        // Check for a flat surface of 4 tiles
//        for (int i = 0; i < 4; i++)
//        {
//            if (!validGroundTypes.Contains(Main.tile[x + i, y].TileType) ||
//                Main.tile[x + i, y - 1].HasTile)
//            {
//                return false;
//            }
//        }

//        // Check for open space above
//        return ScanRectangle(x, y - 6, 4, 6) < 3;
//    }

//    private int ScanRectangle(int x, int y, int width, int height)
//    {
//        int count = 0;
//        for (int i = 0; i < width; i++)
//        {
//            for (int j = 0; j < height; j++)
//            {
//                if (Main.tile[x + i, y + j].HasTile)
//                {
//                    count++;
//                }
//            }
//        }
//        return count;
//    }

//    private void PlaceRandomCrystal(int x, int y)
//    {
//        if (Main.rand.NextBool(2))
//        {
//            int crystalType = Main.rand.Next(3);
//            switch (crystalType)
//            {
//                case 0:
//                    WorldGen.PlaceTile(x, y - 4, ModContent.TileType<EmberiteCrystal_Large1>(), true, true);
//                    break;
//                case 1:
//                    WorldGen.PlaceTile(x, y - 3, ModContent.TileType<EmberiteCrystal_Large2>(), true, true);
//                    break;
//                case 2:
//                    WorldGen.PlaceTile(x, y - 3, ModContent.TileType<EmberiteCrystal_Large3>(), true, true);
//                    break;
//            }
//        }
//        else
//        {
//            int crystalType = Main.rand.Next(3);
//            switch (crystalType)
//            {
//                case 0:
//                    WorldGen.PlaceTile(x, y - 3, ModContent.TileType<EmberiteCrystal_Medium1>(), true, true);
//                    break;
//                case 1:
//                    WorldGen.PlaceTile(x, y - 2, ModContent.TileType<EmberiteCrystal_Medium2>(), true, true);
//                    break;
//                case 2:
//                    WorldGen.PlaceTile(x, y - 1, ModContent.TileType<EmberiteCrystal_Medium3>(), true, true);
//                    break;
//            }
//        }
//    }
//}