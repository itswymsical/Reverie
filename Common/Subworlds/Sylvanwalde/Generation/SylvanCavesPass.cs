using Reverie.lib;
using Reverie.Utilities;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Sylvanwalde.Generation;

public class SylvanCavesPass : GenPass
{
    public SylvanCavesPass() : base("[Sylvan] Caves", 300f)
    {
    }
    private const int CHUNK_SIZE = 64;
    private const float CAVE_THRESHOLD = 0.11f;
    private const float OPEN_CAVE_THRESHOLD = 0.11f;

    private readonly int worldSurfaceLow = (int)(Main.worldSurface + 30);
    private readonly int underworldLayer = Main.UnderworldLayer;

    private static FastNoiseLite SetupCaveNoise()
    {
        var noise = new FastNoiseLite();

        NoiseUtils.ConfigureNoiseBase(noise, FastNoiseLite.NoiseType.OpenSimplex2S, 0.07f, 6);

        return noise;
    }

    private static FastNoiseLite SetupOpenCaveNoise()
    {
        var noise = new FastNoiseLite();

        NoiseUtils.ConfigureNoiseBase(noise, FastNoiseLite.NoiseType.Perlin, 0.036f, 3);

        return noise;
    }

    private static bool ShouldGenerateCave(float caveNoiseValue, float openCaveNoiseValue, int y)
    {
        return caveNoiseValue > CAVE_THRESHOLD ||
               (y >= Main.worldSurface + 30 && openCaveNoiseValue > OPEN_CAVE_THRESHOLD);
    }

    private static void CarveOutCave(int x, int y, float xCave, FastNoiseLite caveNoise, FastNoiseLite openCaveNoise)
    {
        float caveNoiseValue = caveNoise.GetNoise(xCave, y - y / 4f);
        float openCaveNoiseValue = openCaveNoise.GetNoise(x, y);
        Tile tile = Main.tile[x, y];

        if (ShouldGenerateCave(caveNoiseValue, openCaveNoiseValue, y))
        {
            tile.HasTile = false;
        }
    }

    private static void ProcessChunkTile(
        int x,
        int y,
        float xCave,
        float xWall,
        FastNoiseLite caveNoise,
        FastNoiseLite openCaveNoise)
    {
        CarveOutCave(x, y, xCave, caveNoise, openCaveNoise);
    }

    private static void GenerateChunk(
        int i,
        int j,
        int width,
        int height,
        FastNoiseLite caveNoise,
        FastNoiseLite openCaveNoise,
        GenerationProgress progress)
    {
        int endX = Math.Min(i + width, Main.maxTilesX);
        int endY = Math.Min(j + height, Main.UnderworldLayer);
        int totalTiles = (int)(Main.maxTilesX * (Main.UnderworldLayer - (Main.rockLayer - GenVars.worldSurfaceLow)));
        int surfaceOffset = (int)(Main.worldSurface + 30);

        for (int x = i; x < endX; x++)
        {
            float xCave = x / 3f;
            float xWall = x / 2f;

            for (int y = j; y < endY; y++)
            {
                if (y < surfaceOffset)
                    continue;

                ProcessChunkTile(x, y, xCave, xWall, caveNoise, openCaveNoise);

                progress.Set((x * (Main.UnderworldLayer - surfaceOffset) +
                    (y - surfaceOffset)) / totalTiles);
            }
        }
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating Cave Networks";

        var caveNoise = SetupCaveNoise();
        var openCaveNoise = SetupOpenCaveNoise();

        for (int y = worldSurfaceLow; y < underworldLayer; y += CHUNK_SIZE)
        {
            for (int x = 0; x < Main.maxTilesX; x += CHUNK_SIZE)
            {
                GenerateChunk(x, y, CHUNK_SIZE, CHUNK_SIZE, caveNoise, openCaveNoise, progress);
            }
        }
    }
}