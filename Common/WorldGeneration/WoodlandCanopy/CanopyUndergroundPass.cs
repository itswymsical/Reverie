using Reverie.Content.Tiles.Rainforest.Surface;
using Reverie.lib;
using Terraria.GameContent.Biomes;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.WoodlandCanopy;

public class CanopyBounds
{
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int SurfaceY { get; set; }
    public int DepthY { get; set; }
    public bool IsValid => MaxX > MinX && MinX > 0;

    public Rectangle ToRectangle() => new Rectangle(MinX, SurfaceY, MaxX - MinX, DepthY - SurfaceY);
}

public class CanopyUndergroundPass : GenPass
{
    #region Fields
    private CanopyBounds _canopyBounds;
    private FastNoiseLite _caveNoise;
    private FastNoiseLite _densityNoise;
    #endregion

    public CanopyUndergroundPass() : base("Woodland Understory", 150f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Detecting Rainforest boundaries...";

        _canopyBounds = DetectCanopyBounds();

        if (!_canopyBounds.IsValid)
        {
            progress.Message = "No Rainforest biome detected - skipping cave generation";
            return;
        }

        progress.Message = "Growing Woodland Understory...";
        InitializeNoiseGenerators();
        GenerateCanopyCaveSystem(progress);
    }

    private CanopyBounds DetectCanopyBounds()
    {
        var bounds = new CanopyBounds
        {
            MinX = Main.maxTilesX,
            MaxX = 0,
            SurfaceY = Main.maxTilesY,
            DepthY = 0
        };

        // Scan the world for canopy tiles to determine biome boundaries
        for (int x = 200; x < Main.maxTilesX - 200; x += 3) // Sample for efficiency
        {
            for (int y = 50; y < Main.maxTilesY - 100; y += 2)
            {
                if (WorldGen.InWorld(x, y) && IsCanopyTile(x, y))
                {
                    // Found canopy tile - expand bounds
                    bounds.MinX = Math.Min(bounds.MinX, x);
                    bounds.MaxX = Math.Max(bounds.MaxX, x);
                    bounds.SurfaceY = Math.Min(bounds.SurfaceY, y);
                    bounds.DepthY = Math.Max(bounds.DepthY, y);
                }
            }
        }

        // Validate and expand bounds slightly for cave generation
        if (bounds.IsValid)
        {
            bounds.MinX = Math.Max(bounds.MinX - 20, 200);
            bounds.MaxX = Math.Min(bounds.MaxX + 20, Main.maxTilesX - 200);
            bounds.SurfaceY = Math.Max(bounds.SurfaceY - 10, 50);
            bounds.DepthY = Math.Min(bounds.DepthY + 50, Main.maxTilesY - 100);
        }

        return bounds;
    }

    private bool IsCanopyTile(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return false;

        Tile tile = Main.tile[x, y];
        return tile.HasTile && (tile.TileType == TileID.LivingWood ||
                               tile.TileType == (ushort)ModContent.TileType<OxisolTile>() || tile.TileType == (ushort)ModContent.TileType<OxisolTile>());
    }

    private void InitializeNoiseGenerators()
    {
        // Main cave structure noise
        _caveNoise = new FastNoiseLite(WorldGen.genRand.Next());
        _caveNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        _caveNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _caveNoise.SetFrequency(0.087f);
        _caveNoise.SetFractalOctaves(4);

        // Cave density variation noise
        _densityNoise = new FastNoiseLite(WorldGen.genRand.Next());
        _densityNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _densityNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _densityNoise.SetFrequency(0.09f);
        _densityNoise.SetFractalOctaves(2);
    }

    private void GenerateCanopyCaveSystem(GenerationProgress progress)
    {
        Rectangle canopyRect = _canopyBounds.ToRectangle();
        int totalArea = canopyRect.Width * canopyRect.Height;
        int processedTiles = 0;

        // Calculate biome center for distance-based cave density
        int centerX = canopyRect.X + canopyRect.Width / 2;
        int centerY = canopyRect.Y + canopyRect.Height / 2;

        for (int x = canopyRect.Left; x <= canopyRect.Right; x++)
        {
            for (int y = canopyRect.Top; y <= canopyRect.Bottom; y++)
            {
                processedTiles++;
                if (processedTiles % 1000 == 0) // Update progress periodically
                {
                    progress.Set((double)processedTiles / totalArea);
                }

                if (!WorldGen.InWorld(x, y)) continue;

                Tile tile = Main.tile[x, y];

                // Only carve caves in canopy tiles
                if (!tile.HasTile || !IsCanopyTile(x, y)) continue;

                if (ShouldCarveAt(x, y, centerX, centerY, canopyRect))
                {
                    CarveCaveTile(x, y);
                }
            }
        }
    }

    private bool ShouldCarveAt(int x, int y, int centerX, int centerY, Rectangle bounds)
    {
        // Get primary cave noise
        float caveValue = _caveNoise.GetNoise(x / 2.7f, y / 3.7f);

        // Get density variation
        float density = _densityNoise.GetNoise(x, y) * 0.5f + 0.5f; // Remap to 0-1

        // Calculate distance factors for varied cave density
        float distFromCenterX = (x - centerX) / (float)bounds.Width;
        float distFromCenterY = (y - centerY) / (float)bounds.Height;
        float distFromCenter = (float)Math.Sqrt(distFromCenterX * distFromCenterX + distFromCenterY * distFromCenterY);

        // Create depth-based cave density (more caves deeper underground)
        float depthFactor = (y - bounds.Top) / (float)bounds.Height;
        depthFactor = Math.Min(1.0f, depthFactor * 1.5f); // Increase cave likelihood with depth

        // Edge tapering - reduce caves near biome edges
        float edgeFactor = CalculateEdgeFactor(x, y, bounds);

        // Combine all factors for final threshold
        float combinedFactor = edgeFactor * (0.7f + depthFactor * 0.3f);
        float threshold = 0.25f - (density * combinedFactor * 0.4f);

        return caveValue > threshold;
    }

    private float CalculateEdgeFactor(int x, int y, Rectangle bounds)
    {
        // Calculate distance from edges
        float distFromLeftEdge = (x - bounds.Left) / (float)bounds.Width;
        float distFromRightEdge = (bounds.Right - x) / (float)bounds.Width;
        float distFromTopEdge = (y - bounds.Top) / (float)bounds.Height;
        float distFromBottomEdge = (bounds.Bottom - y) / (float)bounds.Height;

        // Use minimum distance to any edge
        float minDistFromEdge = Math.Min(Math.Min(distFromLeftEdge, distFromRightEdge),
                                        Math.Min(distFromTopEdge, distFromBottomEdge));

        // Create smooth falloff at edges
        float edgeTaperZone = 0.15f; // 15% of biome width/height
        if (minDistFromEdge < edgeTaperZone)
        {
            return (float)(0.5 * (1 + Math.Cos(Math.PI * (1 - minDistFromEdge / edgeTaperZone))));
        }

        return 1.0f; // Full cave generation in center areas
    }

    private void CarveCaveTile(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        tile.HasTile = false;
        tile.IsHalfBlock = false;
        tile.Slope = SlopeType.Solid;

        // Optional: Add air walls for better cave appearance
        if (tile.WallType == 0)
        {
            // You could set a specific wall type here if desired
            // tile.WallType = WallID.SomeWallType;
        }
    }

    #region Validation
    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    }
    #endregion
}