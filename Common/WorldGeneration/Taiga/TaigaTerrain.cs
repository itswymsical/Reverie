using Reverie.Content.Tiles.Taiga;
using Reverie.lib;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TaigaTerrain : GenPass
{
    private readonly FastNoiseLite _terrainNoise;
    private readonly FastNoiseLite _detailNoise;

    public TaigaTerrain() : base("[Reverie] Taiga", 247.43f)
    {
        _terrainNoise = new FastNoiseLite(WorldGen.genRand.Next());
        _terrainNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _terrainNoise.SetFrequency(0.008f);
        _terrainNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _terrainNoise.SetFractalOctaves(4);

        _detailNoise = new FastNoiseLite(WorldGen.genRand.Next());
        _detailNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _detailNoise.SetFrequency(0.04f);
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        if (!WorldGen.remixWorldGen)
        {
            GenerateSnowBiome(progress, configuration);

            progress.Message = "Growing Taiga";
        }
    }
    private void GenerateSnowBiome(GenerationProgress progress, GameConfiguration passConfig)
    {
        progress.Message = Lang.gen[56].Value; // "Generating Snow"

        GenVars.snowTop = (int)Main.worldSurface;
        int lowerBound = GenVars.lavaLine - WorldGen.genRand.Next(160, 200);
        int upperBound = GenVars.lavaLine;

        int leftX = GenVars.snowOriginLeft;
        int rightX = GenVars.snowOriginRight;
        int width = 10;

        for (int y = 0; y <= upperBound - 140; y++)
        {
            progress.Set((double)y / (double)(upperBound - 140));

            leftX += WorldGen.genRand.Next(-4, 4);
            rightX += WorldGen.genRand.Next(-3, 5);

            if (y > 0)
            {
                leftX = (leftX + GenVars.snowMinX[y - 1]) / 2;
                rightX = (rightX + GenVars.snowMaxX[y - 1]) / 2;
            }

            if (GenVars.dungeonSide > 0)
            {
                if (WorldGen.genRand.NextBool(4))
                {
                    leftX++;
                    rightX++;
                }
            }
            else if (WorldGen.genRand.NextBool(4))
            {
                leftX--;
                rightX--;
            }

            GenVars.snowMinX[y] = leftX;
            GenVars.snowMaxX[y] = rightX;

            for (int x = leftX; x < rightX; x++)
            {
                if (y < lowerBound)
                {
                    ConvertToTaigaTile(x, y);
                }
                else
                {
                    width += WorldGen.genRand.Next(-3, 4);
                    if (WorldGen.genRand.NextBool(3))
                    {
                        width += WorldGen.genRand.Next(-4, 5);
                        if (WorldGen.genRand.NextBool(3))
                            width += WorldGen.genRand.Next(-6, 7);
                    }

                    if (width < 0)
                        width = WorldGen.genRand.Next(3);
                    else if (width > 50)
                        width = 50 - WorldGen.genRand.Next(3);

                    for (int depth = y; depth < y + width; depth++)
                    {
                        ConvertToTaigaTile(x, depth);
                    }
                }
            }

            if (GenVars.snowBottom < y)
                GenVars.snowBottom = y;
        }
    }

    private void ConvertToTaigaTile(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return;

        Tile tile = Main.tile[x, y];

        if (tile.WallType == WallID.DirtUnsafe)
            tile.WallType = WallID.SnowWallUnsafe;

        float noiseValue = _detailNoise.GetNoise(x, y) * 0.5f + 0.5f;

        switch (tile.TileType)
        {
            case TileID.Dirt:
            case TileID.Grass:
                if (y < GenVars.snowTop + 40)
                {
                    tile.TileType = (ushort)ModContent.TileType<PeatTile>();
                }
                else
                {
                    if (noiseValue < 0.37f)
                        tile.TileType = TileID.Slush;
                    else
                        tile.TileType = TileID.SnowBlock;
                }
                break;

            case TileID.Stone:
                if (noiseValue > 0.495f)
                    tile.TileType = TileID.SnowBlock;
                else
                {
                    tile.TileType = TileID.IceBlock;
                }
                break;
            case TileID.Sand:
                if (noiseValue > 0.54f)
                    tile.TileType = TileID.Slush;
                else
                {
                    tile.TileType = TileID.SnowBlock;
                }
                break;
        }
    }
}
