using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.BiomeTypes;

/// <summary>
/// Example implementation for simple spawn-proximity biomes
/// </summary>
public abstract class FilterBiome : SurfaceBiomeBase
{
    protected FilterBiome(string name, float weight, BiomeConfiguration config = null)
        : base(name, weight, config) { }

    protected override bool GetBiomeBounds(out BiomeBounds bounds)
    {
        bounds = default;
        
        int width = WorldGen.genRand.Next(_config.MinWidth, _config.MaxWidth);
        int minDistance = (int)(Main.maxTilesX * 0.009f);
        int maxDistance = (int)(Main.maxTilesX * 0.045f);

        Rectangle spawnBounds = CalculateSpawnProximityBounds(minDistance, maxDistance, width);
        if (spawnBounds.IsEmpty) return false;

        bounds = new BiomeBounds
        {
            Left = spawnBounds.Left,
            Right = spawnBounds.Right,
            Top = (int)Main.worldSurface,
            Bottom = (int)Main.worldSurface + _config.SurfaceDepth
        };

        return bounds.IsValid;
    }

    protected override void PopulateBiome(GenerationProgress progress)
    {
        int depth = (int)Main.worldSurface + 130;

    }
}
