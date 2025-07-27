namespace Reverie.Common.WorldGeneration.BiomeTypes;

/// <summary>
/// Example implementation for complex biomes that avoid other biomes
/// </summary>
public abstract class TransitionBiome : SurfaceBiomeBase
{
    protected TransitionBiome(string name, float weight, BiomeConfiguration config = null)
        : base(name, weight, config) { }

    protected override bool GetBiomeBounds(out BiomeBounds bounds)
    {
        bounds = default;

        var conflictBiomes = GetConflictBiomes();
        if (!FindSuitableLocation(conflictBiomes, out Rectangle area))
            return false;

        bounds = new BiomeBounds
        {
            Left = area.Left,
            Right = area.Right,
            Top = (int)Main.worldSurface - 50,
            Bottom = (int)Main.worldSurface + _config.SurfaceDepth
        };

        return ValidatePlacement(bounds);
    }

    protected abstract int[] GetConflictBiomes();

    protected virtual bool FindSuitableLocation(int[] conflictBiomes, out Rectangle area)
    {
        area = Rectangle.Empty;
        int width = WorldGen.genRand.Next(_config.MinWidth, _config.MaxWidth);

        for (int startX = 200; startX < Main.maxTilesX - width - 200; startX += 50)
        {
            if (IsNearBiome(startX, width, conflictBiomes))
                continue;

            if (IsSuitableForPlacement(startX, width))
            {
                area = new Rectangle(startX, 0, width, 0);
                return true;
            }
        }

        return false;
    }

    protected virtual bool ValidatePlacement(BiomeBounds bounds) => true;

    protected virtual bool IsSuitableForPlacement(int startX, int width)
    {
        int surfaceY = (int)Main.worldSurface;
        int suitableCount = 0;
        int totalSamples = 20;

        for (int i = 0; i < totalSamples; i++)
        {
            int x = startX + (width * i / totalSamples);
            if (!WorldGen.InWorld(x, surfaceY)) continue;

            Tile tile = Main.tile[x, surfaceY];
            if (CanReplaceTile(tile.TileType))
                suitableCount++;
        }

        return (double)suitableCount / totalSamples >= 0.7;
    }
}