using Reverie.lib;
using Terraria.IO;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;

namespace Reverie.Common.Subworlds.Sylvanwalde.Generation.DruidsGarden;

public class DGCavePass : GenPass
{
    private FastNoiseLite _caveNoise;

    // Cave configuration
    private float _noiseFrequency = 0.02f;
    private int _caveHeight = 15;      // Base height of the cave
    private float _caveVariation = 5f; // How much the cave height can vary
    private float _threshold = 0.3f;   // Threshold for cave generation (higher = smaller cave)

    public DGCavePass() : base("[Sylvan] Horizontal Cave", 300f)
    {
        InitializeNoise();
    }

    private void InitializeNoise()
    {
        _caveNoise = new FastNoiseLite();
        _caveNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _caveNoise.SetFrequency(_noiseFrequency);
        _caveNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _caveNoise.SetFractalOctaves(4);
        _caveNoise.SetSeed(WorldGen.genRand.Next(100000));
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating Druid's Garden...";

        int surfaceLevel = (int)Main.worldSurface + 40;

        surfaceLevel = Math.Clamp(surfaceLevel, 100, Main.maxTilesY - 200);

        for (int x = 20; x < Main.maxTilesX - 20; x++)
        {
            if (x % 100 == 0)
            {
                progress.Set((float)x / Main.maxTilesX);
            }

            float noiseValue = _caveNoise.GetNoise(x * 0.5f, 0); // Only vary by x
            int caveCenterY = surfaceLevel + (int)(noiseValue * _caveVariation * 2);

            caveCenterY = Math.Clamp(caveCenterY, 50, Main.maxTilesY - 50);

            CarveVerticalSection(x, caveCenterY);
        }

        AddRootBranches(surfaceLevel);

        progress.Set(1f);
    }

    private void CarveVerticalSection(int x, int centerY)
    {
        // Calculate the height of the cave at this position using a second noise layer
        float heightNoise = _caveNoise.GetNoise(x * 0.2f, 100); // Different seed for height variation
        int caveHeight = (int)(_caveHeight + heightNoise * _caveVariation);

        // Calculate the top and bottom of the cave
        int topY = centerY - caveHeight / 2;
        int bottomY = centerY + caveHeight / 2;

        // Make sure we're within world bounds
        topY = Math.Max(20, topY);
        bottomY = Math.Min(Main.maxTilesY - 20, bottomY);

        // Carve the cave
        for (int y = topY; y <= bottomY; y++)
        {
            // Calculate distance from center for a more natural cave shape
            float distFromCenter = Math.Abs((y - centerY) / (float)(caveHeight / 2));

            // Use noise to determine the edge of the cave
            float edgeNoise = _caveNoise.GetNoise(x * 0.1f, y * 0.1f);

            // If we're within the cave boundary, remove the tile
            if (distFromCenter + edgeNoise * 0.5f < 1.0f - _threshold)
            {
                // Make sure the tile exists before trying to remove it
                if (IsValidPosition(x, y))
                {
                    Main.tile[x, y].ClearTile();
                    Main.tile[x, y].WallType = 0; // Remove walls too
                }
            }
        }
    }

    private void AddRootBranches(int surfaceLevel)
    {
        int branchCount = (int)(Main.maxTilesX * 0.015f); // 1.5% of world width

        for (int i = 0; i < branchCount; i++)
        {
            // Pick a starting point along the main cave with a safe margin
            int startX = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
            int startY = surfaceLevel + WorldGen.genRand.Next(-10, 11); // Narrower range near surface level

            // Ensure this point is valid
            if (!IsValidPosition(startX, startY))
            {
                continue;
            }

            // Make sure the starting point is empty (in the existing cave)
            if (IsValidPosition(startX, startY) && !Main.tile[startX, startY].HasTile)
            {
                // Create a root branch using the ShapeRoot

                // Randomize parameters
                double angle = WorldGen.genRand.NextDouble() * Math.PI * 2; // Random direction (0-2π)
                double distance = WorldGen.genRand.NextDouble() * 30.0 + 20.0; // Distance between 20-50
                double startingSize = WorldGen.genRand.NextDouble() * 3.0 + 2.0; // Starting size between 2-5
                double endingSize = WorldGen.genRand.NextDouble() * 1.5 + 0.5; // Ending size between 0.5-2

                // Create the shape and action
                ShapeRoot shape = new ShapeRoot(angle, distance, startingSize, endingSize);

                // The action to clear tiles
                ClearTileAction clearAction = new ClearTileAction();

                // Create the root starting at this position
                shape.Perform(new Point(startX, startY), clearAction);
            }
        }
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    }
}

public class ClearTileAction : GenAction
{
    public override bool Apply(Point origin, int x, int y, params object[] args)
    {
        // Make sure the position is valid
        if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
            return false;

        // Clear the tile and wall
        Main.tile[x, y].ClearTile();
        Main.tile[x, y].WallType = 0;

        return true;
    }
}