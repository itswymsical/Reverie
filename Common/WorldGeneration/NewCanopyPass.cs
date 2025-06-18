using Reverie.lib;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;

//public class NewCanopyPass : GenPass
//{
//    public NewCanopyPass() : base("Canopy", 124f)
//    {
//    }

//    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
//    {
//        progress.Message = "Generating Canopy";

//        int worldWidth = Main.maxTilesX;
//        int worldHeight = Main.maxTilesY;
//        int canopyWidth = (int)(worldWidth * 0.055f);
//        int canopyHeight = (int)(worldHeight * 0.45f);

//        int centerX = Main.spawnTileX - 50;
//        int centerY = worldHeight / 2;

//        float curveFrequency = 0.05f;
//        int curveAmplitude = canopyWidth / 10;
//        int thornHeight = canopyHeight / 24;
//        int thornWidth = canopyWidth / 16;

//        GenerateRoundedCanopy(centerX, centerY, canopyWidth, canopyHeight,
//                       curveFrequency, curveAmplitude, thornHeight, thornWidth);

//        GenerateCaverns(centerX, centerY, canopyWidth, canopyHeight);

//        SmoothCanopy(centerX, centerY, canopyWidth, canopyHeight);
//    }

//    private void GenerateRoundedCanopy(int centerX, int centerY, int width, int height,
//                              float curveFrequency, int curveAmplitude,
//                              int thornHeight, int thornWidth)
//    {
//        int left = centerX - width / 2 - curveAmplitude - thornWidth;
//        int right = centerX + width / 2 + curveAmplitude + thornWidth;
//        int top = centerY - height / 2 - thornHeight;
//        int bottom = centerY + height / 2 + thornHeight;

//        left = Math.Max(left, 0);
//        right = Math.Min(right, Main.maxTilesX - 1);
//        top = Math.Max(top, 0);
//        bottom = Math.Min(bottom, Main.maxTilesY - 1);

//        for (int y = top; y <= bottom; y++)
//        {
//            for (int x = left; x <= right; x++)
//            {
//                if (IsPointInRoundedCanopy(x, y, centerX, centerY, width, height,
//                                      curveFrequency, curveAmplitude,
//                                      thornHeight, thornWidth))
//                {
//                    PlaceTile(x, y, TileID.LivingWood);
//                }
//            }
//        }
//    }

//    private bool IsPointInRoundedCanopy(int x, int y, int centerX, int centerY, int width, int height,
//                                        float curveFrequency, int curveAmplitude,
//                                        int thornHeight, int thornWidth)
//    {
//        float relativeY = (float)(y - centerY) / (height / 2);

//        if (Math.Abs(relativeY) >= 1.0f)
//            return false;

//        // Calculate smoothly varying width based on elliptical equation
//        // x²/a² + y²/b² = 1 -> x² = a²(1 - y²/b²) -> x = a * sqrt(1 - y²/b²)
//        float ellipticalFactor = (float)Math.Sqrt(1.0f - relativeY * relativeY);

//        // Apply wave distortion that scales with distance from center
//        float waveIntensity = 1.0f - 0.7f * Math.Abs(relativeY);
//        float yOffset = (y - centerY) * curveFrequency;
//        int waveOffset = (int)(Math.Sin(yOffset) * curveAmplitude * waveIntensity);
//        int waveX = centerX + waveOffset;

//        int halfWidth = (int)(width / 2 * ellipticalFactor);

//        int seedValue = y * 1000 + x;
//        Random rand = new Random(Main.ActiveWorldFileData.Seed + seedValue);
//        float randomFactor = (float)(rand.NextDouble() * 0.15 + 0.93);
//        halfWidth = (int)(halfWidth * randomFactor);

//        // Check if point is within the main elliptical body
//        if (x >= waveX - halfWidth && x <= waveX + halfWidth)
//        {
//            return true;
//        }

//        if (Math.Abs(relativeY) < 0.85f)
//        {
//            int scaledThornHeight = (int)(thornHeight * (1.0f - 0.7f * Math.Abs(relativeY)));
//            int scaledThornWidth = (int)(thornWidth * (1.0f - 0.5f * Math.Abs(relativeY)));

//            bool isPeak = Math.Abs(Math.Sin(yOffset) - 1.0f) < 0.1f;
//            bool isValley = Math.Abs(Math.Sin(yOffset) + 1.0f) < 0.1f;

//            if (isPeak || isValley)
//            {
//                int thornDirection = isPeak ? -1 : 1;
//                int thornX = waveX + (thornDirection * halfWidth);
//                int thornTipX = thornX + (thornDirection * scaledThornWidth);

//                int minX = Math.Min(thornX, thornTipX);
//                int maxX = Math.Max(thornX, thornTipX);

//                if (x >= minX && x <= maxX)
//                {
//                    float thornProgress = (float)(x - thornX) / (thornTipX - thornX);
//                    int thornMinY = y - scaledThornHeight / 2;
//                    int thornMaxY = y + scaledThornHeight / 2;
//                    int thornMiddleY = y;

//                    int thornY;
//                    if (isPeak)
//                    {
//                        thornY = (int)(thornMiddleY - Math.Abs(thornProgress) * scaledThornHeight);
//                    }
//                    else
//                    {
//                        thornY = (int)(thornMiddleY + Math.Abs(thornProgress) * scaledThornHeight);
//                    }

//                    if ((isPeak && y >= thornY && y <= thornMiddleY) ||
//                        (!isPeak && y <= thornY && y >= thornMiddleY))
//                    {
//                        return true;
//                    }
//                }
//            }
//        }

//        return false;
//    }

//    private void GenerateCaverns(int centerX, int centerY, int width, int height)
//    {
//        // Set up noise generator
//        FastNoiseLite noise = new FastNoiseLite(WorldGen.genRand.Next());
//        noise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
//        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
//        noise.SetFrequency(0.07f);
//        noise.SetFractalOctaves(4);

//        int left = centerX - width / 2 - 10;
//        int right = centerX + width / 2 + 10;
//        int top = centerY - height / 2 - 10;
//        int bottom = centerY + height / 2 + 10;

//        // Ensure we're within world bounds
//        left = Math.Max(left, 0);
//        right = Math.Min(right, Main.maxTilesX - 1);
//        top = Math.Max(top, 0);
//        bottom = Math.Min(bottom, Main.maxTilesY - 1);

//        // Use another noise for variable cave density
//        FastNoiseLite densityNoise = new FastNoiseLite(WorldGen.genRand.Next());
//        densityNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
//        densityNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
//        densityNoise.SetFrequency(0.08f);

//        // Carve caves only where there are tiles and it's within our biome
//        for (int x = left; x <= right; x++)
//        {
//            for (int y = top; y <= bottom; y++)
//            {
//                Tile tile = Main.tile[x, y];

//                // Only process tiles that exist
//                if (!tile.HasTile) continue;

//                // Check if this tile is part of our biome
//                if (tile.TileType != TileID.LivingWood && tile.TileType != TileID.Dirt) continue;

//                // Get noise values
//                float noiseValue = noise.GetNoise(x / 2, y / 1.7f);
//                float density = densityNoise.GetNoise(x, y) * 0.5f + 0.5f; // Remap to 0-1

//                // Calculate distance from center for varied cave density
//                float distFromCenterX = (x - centerX) / (float)width;
//                float distFromCenterY = (y - centerY) / (float)height;
//                float distFromCenter = (float)Math.Sqrt(distFromCenterX * distFromCenterX + distFromCenterY * distFromCenterY);

//                // Make caves more likely in center, less likely at edges
//                float edgeFactor = 1.0f - distFromCenter * 1.8f;
//                edgeFactor = Math.Max(0.1f, edgeFactor); // Keep a minimum chance even at edges

//                // Variable threshold based on position and density noise
//                float threshold = 0.25f - (density * edgeFactor * 0.5f);

//                // Create caves where noise is above threshold
//                if (noiseValue > threshold)
//                {
//                    tile.HasTile = false;
//                }
//            }
//        }
//    }

//    private void SmoothCanopy(int centerX, int centerY, int width, int height)
//    {
//        int smoothRadius = 2;
//        int smoothPasses = 3;

//        int left = centerX - width / 2 - smoothRadius * 3;
//        int right = centerX + width / 2 + smoothRadius * 3;
//        int top = centerY - height / 2 - smoothRadius * 3;
//        int bottom = centerY + height / 2 + smoothRadius * 3;

//        left = Math.Max(left, 0);
//        right = Math.Min(right, Main.maxTilesX - 1);
//        top = Math.Max(top, 0);
//        bottom = Math.Min(bottom, Main.maxTilesY - 1);

//        for (int pass = 0; pass < smoothPasses; pass++)
//        {
//            bool[,] tilePresent = new bool[right - left + 1, bottom - top + 1];

//            for (int x = left; x <= right; x++)
//            {
//                for (int y = top; y <= bottom; y++)
//                {
//                    tilePresent[x - left, y - top] = Main.tile[x, y].HasTile;
//                }
//            }

//            for (int x = left + 1; x < right; x++)
//            {
//                for (int y = top + 1; y < bottom; y++)
//                {
//                    int neighbors = 0;

//                    for (int nx = -1; nx <= 1; nx++)
//                    {
//                        for (int ny = -1; ny <= 1; ny++)
//                        {
//                            if (nx == 0 && ny == 0) continue;

//                            if (tilePresent[x + nx - left, y + ny - top])
//                            {
//                                neighbors++;
//                            }
//                        }
//                    }

//                    if (!tilePresent[x - left, y - top] && neighbors >= 5)
//                    {
//                        PlaceTile(x, y, TileID.LivingWood);
//                    }
//                    else if (tilePresent[x - left, y - top] && neighbors <= 2)
//                    {
//                        Tile tile = Main.tile[x, y];
//                        tile.HasTile = false;
//                    }
//                }
//            }
//        }
//    }
//    private void PlaceTile(int x, int y, int tileType)
//    {
//        Tile tile = Main.tile[x, y];
//        tile.HasTile = true;
//        tile.TileType = (ushort)tileType;

//        if (tileType == TileID.Dirt || tileType == TileID.LivingWood)
//        {
//            WorldGen.TileFrame(x, y);
//        }
//    }
//}