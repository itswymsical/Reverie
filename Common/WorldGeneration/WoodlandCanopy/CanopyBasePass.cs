using Reverie.Content.Tiles.Canopy.Surface;
using Reverie.lib;
using Terraria.GameContent.Biomes;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.WoodlandCanopy;

public class CanopyConfiguration
{
    public float HillFrequency { get; set; } = 0.0022f;
    public int BaseHeightOffset { get; set; } = 6;
    public int HillHeightVariation { get; set; } = 15;
    public int CanopyDepth { get; set; } = (int)(Main.maxTilesY / 1.7647f);
}

public class JungleBounds
{
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int SurfaceY { get; set; }
    public int Height { get; set; }
    public bool IsValid => MaxX > MinX && MinX > 0;

    public Rectangle ToRectangle() => new Rectangle(MinX, SurfaceY, MaxX - MinX, Height);
}

public class CanopyBasePass : GenPass
{
    #region Fields
    private readonly CanopyConfiguration _config;
    private FastNoiseLite _hillNoise;
    private int _canopyLeft, _canopyRight;
    private JungleBounds _jungleBounds;
    #endregion

    public CanopyBasePass() : base("Canopy Overworld", 80f)
    {
        _config = new CanopyConfiguration();
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Analyzing biome boundaries...";

        Initialize();

        // Multi-layered jungle detection approach
        _jungleBounds = DetectJungleBoundaries();

        if (!_jungleBounds.IsValid)
        {
            progress.Message = "Failed to detect jungle boundaries - skipping Canopy generation";
            return;
        }

        progress.Message = "Growing Canopy Biome adjacent to jungle...";
        CalculateCanopyBounds();
        GenerateCanopyTerrain(progress);
    }

    private void Initialize()
    {
        _hillNoise = new FastNoiseLite();
        _hillNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        _hillNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _hillNoise.SetFractalOctaves(3);
        _hillNoise.SetFrequency(_config.HillFrequency);
    }

    private JungleBounds DetectJungleBoundaries()
    {
        // Layer 1: Validate GenVars jungle coordinates
        JungleBounds bounds = ValidateGenVarsJungle();
        if (bounds.IsValid)
        {
            return bounds;
        }

        // Layer 2: Comprehensive tile scanning
        bounds = ScanForJungleBiome();
        if (bounds.IsValid)
        {
            return bounds;
        }

        // Layer 3: Positional estimation based on Terraria's generation rules
        return EstimateJungleLocation();
    }

    private JungleBounds ValidateGenVarsJungle()
    {
        var bounds = new JungleBounds();

        // Validate GenVars coordinates with bounds checking
        if (GenVars.jungleMinX > 0 && GenVars.jungleMaxX > GenVars.jungleMinX &&
            GenVars.jungleMaxX < Main.maxTilesX - 200)
        {
            bounds.MinX = GenVars.jungleMinX;
            bounds.MaxX = GenVars.jungleMaxX;
            bounds.SurfaceY = (int)Main.worldSurface;
            bounds.Height = 400;

            // Verify by checking if area actually contains jungle tiles
            if (ValidateJungleArea(bounds.MinX + (bounds.MaxX - bounds.MinX) / 2, bounds.SurfaceY + 50))
            {
                return bounds;
            }
        }

        return new JungleBounds(); // Invalid
    }

    private JungleBounds ScanForJungleBiome()
    {
        var bounds = new JungleBounds
        {
            MinX = Main.maxTilesX,
            MaxX = 0,
            SurfaceY = (int)Main.worldSurface,
            Height = 400
        };

        int scanStartY = (int)Main.worldSurface;
        int scanEndY = Math.Min((int)Main.worldSurface + 200, Main.maxTilesY - 100);

        // Scan for jungle tiles in surface area
        for (int x = 200; x < Main.maxTilesX - 200; x += 5) // Sample every 5 tiles for efficiency
        {
            for (int y = scanStartY; y < scanEndY; y += 3)
            {
                if (WorldGen.InWorld(x, y) && IsJungleTile(x, y))
                {
                    // Verify this is a significant jungle area, not just scattered tiles
                    if (ValidateJungleArea(x, y, 25))
                    {
                        bounds.MinX = Math.Min(bounds.MinX, x);
                        bounds.MaxX = Math.Max(bounds.MaxX, x);
                    }
                }
            }
        }

        // Expand bounds slightly to ensure we capture the full biome
        if (bounds.IsValid)
        {
            bounds.MinX = Math.Max(bounds.MinX - 50, 200);
            bounds.MaxX = Math.Min(bounds.MaxX + 50, Main.maxTilesX - 200);
        }

        return bounds;
    }

    private JungleBounds EstimateJungleLocation()
    {
        // Jungle typically spawns opposite side from Dungeon
        bool jungleOnLeft = Main.dungeonX > Main.maxTilesX / 2;
        int estimatedCenter = jungleOnLeft ? Main.maxTilesX / 4 : (Main.maxTilesX * 3) / 4;
        int estimatedWidth = Main.maxTilesX / 5; // Jungle typically spans ~20% of world width

        return new JungleBounds
        {
            MinX = Math.Max(estimatedCenter - estimatedWidth / 2, 200),
            MaxX = Math.Min(estimatedCenter + estimatedWidth / 2, Main.maxTilesX - 200),
            SurfaceY = (int)Main.worldSurface,
            Height = 400
        };
    }

    private bool IsJungleTile(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return false;

        Tile tile = Main.tile[x, y];
        return tile.HasTile && (tile.TileType == TileID.JungleGrass ||
                               tile.TileType == TileID.Mud ||
                               tile.TileType == TileID.JunglePlants ||
                               tile.TileType == TileID.JungleVines);
    }

    private bool ValidateJungleArea(int centerX, int centerY, int radius = 25)
    {
        int jungleTiles = 0;
        int totalTiles = 0;

        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (WorldGen.InWorld(x, y))
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile)
                    {
                        totalTiles++;
                        if (IsJungleTile(x, y))
                        {
                            jungleTiles++;
                        }
                    }
                }
            }
        }

        return totalTiles > 0 && (float)jungleTiles / totalTiles > 0.25f; // 25% jungle tile threshold
    }

    private void CalculateCanopyBounds()
    {
        // Analyze jungle edges to determine optimal canopy placement
        Rectangle jungleRect = _jungleBounds.ToRectangle();
        int jungleCenter = (jungleRect.Left + jungleRect.Right) / 2;
        int worldCenter = Main.maxTilesX / 2;

        // Determine which side has more space for canopy placement
        int leftSpace = Math.Max(0, jungleRect.Left - 200);
        int rightSpace = Math.Max(0, (Main.maxTilesX - 200) - jungleRect.Right);

        // Choose side based on available space and world position
        bool placeOnLeft;
        if (Math.Abs(leftSpace - rightSpace) > 100)
        {
            placeOnLeft = leftSpace > rightSpace;
        }
        else
        {
            // If spaces are similar, place on side away from world center
            placeOnLeft = jungleCenter > worldCenter;
        }

        // Calculate canopy dimensions
        int canopyWidth = WorldGen.genRand.Next((int)(Main.maxTilesX / 20.33f), (int)(Main.maxTilesX / 10.43f));
        int jungleOffset = -160; // Small gap between jungle and canopy

        // Position canopy with proper spacing analysis
        if (placeOnLeft)
        {
            _canopyRight = jungleRect.Left - jungleOffset;
            _canopyLeft = Math.Max(_canopyRight - canopyWidth, 200);

            // If there's not enough space, adjust the width
            if (_canopyLeft <= 200)
            {
                _canopyLeft = 200;
                canopyWidth = _canopyRight - _canopyLeft;
            }
        }
        else
        {
            _canopyLeft = jungleRect.Right + jungleOffset;
            _canopyRight = Math.Min(_canopyLeft + canopyWidth, Main.maxTilesX - 200);

            // If there's not enough space, adjust the width
            if (_canopyRight >= Main.maxTilesX - 200)
            {
                _canopyRight = Main.maxTilesX - 200;
                canopyWidth = _canopyRight - _canopyLeft;
            }
        }

        // Ensure minimum viable width
        if (canopyWidth < 100)
        {
            throw new InvalidOperationException("Insufficient space for Canopy biome placement adjacent to jungle");
        }
    }

    private void GenerateCanopyTerrain(GenerationProgress progress)
    {
        int totalColumns = _canopyRight - _canopyLeft;

        for (int x = _canopyLeft; x < _canopyRight; x++)
        {
            if (!IsValidX(x)) continue;

            progress.Set((double)(x - _canopyLeft) / totalColumns);
            GenerateCanopyColumn(x);
        }
    }

    private void GenerateCanopyColumn(int x)
    {
        int anchorSurface = ScanForAnchorSurface(x);
        if (anchorSurface == -1) return; // No anchor surface found

        int canopyHeight = CalculateCanopyHeight(x, anchorSurface);
        ClearAndFillCanopyColumn(x, canopyHeight, anchorSurface);
    }

    private int ScanForAnchorSurface(int x)
    {
        // Scan downward from sky to find the first solid tile as our anchor
        for (int y = Main.maxTilesY / 16; y < Main.maxTilesY - 200; y++)
        {
            if (WorldGen.InWorld(x, y) && Main.tile[x, y].HasTile)
            {
                return y; // Found our anchor tile
            }
        }
        return -1; // No anchor found
    }

    private int CalculateCanopyHeight(int x, int anchorSurface)
    {
        // Calculate edge distance for smooth tapering
        float distanceFromLeft = (float)(x - _canopyLeft);
        float distanceFromRight = (float)(_canopyRight - x);
        float minDistanceFromEdge = Math.Min(distanceFromLeft, distanceFromRight);
        float biomeWidth = _canopyRight - _canopyLeft;

        // Create smooth taper zones at edges
        float taperZone = Math.Min(80f, biomeWidth * 0.2f); // 20% of biome width or 80 tiles max
        float edgeFactor = Math.Min(1f, minDistanceFromEdge / taperZone);

        // Use smooth cosine curve for more natural tapering
        edgeFactor = (float)(0.5 * (1 + Math.Cos(Math.PI * (1 - edgeFactor))));

        // Generate base terrain with multiple noise octaves for natural look
        float primaryNoise = _hillNoise.GetNoise(x * 0.5f, 0f);
        float secondaryNoise = _hillNoise.GetNoise(x * 1.0f, 100f) * 0.2f;
        float combinedNoise = primaryNoise + secondaryNoise;

        // Apply edge tapering to height variation
        float taperedHeightVariation = _config.HillHeightVariation * edgeFactor;

        // Calculate new canopy surface height relative to anchor
        int baseHeight = anchorSurface - _config.BaseHeightOffset;
        int heightVariation = (int)(combinedNoise * taperedHeightVariation);

        // Ensure smooth transition at edges by blending with anchor surface
        if (edgeFactor < 0.35f) // Near edges, blend more with existing terrain
        {
            float blendFactor = edgeFactor / 0.2f;
            int blendedHeight = (int)(baseHeight * blendFactor + anchorSurface * (1f - blendFactor));
            return Math.Max(blendedHeight + heightVariation, anchorSurface - 60);
        }

        return Math.Clamp(baseHeight + heightVariation, anchorSurface - 60, anchorSurface + 30);
    }

    private void ClearAndFillCanopyColumn(int x, int canopyHeight, int anchorSurface)
    {
        // Clear existing tiles above our new surface to prevent conflicts
        for (int y = Math.Max(canopyHeight - 10, anchorSurface - 50); y < canopyHeight; y++)
        {
            if (WorldGen.InWorld(x, y))
            {
                Main.tile[x, y].ClearTile();
            }
        }

        // Fill the canopy column
        FillCanopyColumn(x, canopyHeight);
    }

    private void FillCanopyColumn(int x, int hillHeight)
    {
        int maxDepth = hillHeight + _config.CanopyDepth;

        for (int y = hillHeight; y < maxDepth && y < Main.maxTilesY; y++)
        {
            if (!IsValidY(y)) continue;

            PlaceCanopyTile(x, y, hillHeight);
        }
    }

    private void PlaceCanopyTile(int x, int y, int surfaceY)
    {
        Tile tile = Main.tile[x, y];
        tile.HasTile = true;

        int depthFromSurface = y - surfaceY;
        if (depthFromSurface <= 10)
        {
            tile.TileType = (ushort)ModContent.TileType<OxisolTile>();
        }
        else if (depthFromSurface <= 25)
        {
            tile.TileType = TileID.ClayBlock;
        }
        else if (depthFromSurface <= 240)
        {
            tile.TileType = TileID.Mud;
        }
        else if (depthFromSurface <= 280)
        {
            tile.TileType = (ushort)ModContent.TileType<OxisolTile>();
        }
        else
        {
            tile.TileType = TileID.LivingWood;
        }
    }

    #region Validation
    private bool IsValidX(int x)
    {
        return x >= 0 && x < Main.maxTilesX;
    }

    private bool IsValidY(int y)
    {
        return y >= 0 && y < Main.maxTilesY;
    }
    #endregion
}