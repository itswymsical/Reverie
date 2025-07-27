using Terraria.WorldBuilding;

namespace Reverie.Utilities;
public class JungleBounds
{
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int SurfaceY { get; set; }
    public int Height { get; set; }
    public bool IsValid => MaxX > MinX && MinX > 0;

    public Rectangle ToRectangle() => new Rectangle(MinX, SurfaceY, MaxX - MinX, Height);
}

public static class JungleDetection
{
    public static JungleBounds DetectJungleBoundaries()
    {
        // Layer 1: Validate GenVars jungle coordinates
        var bounds = ValidateGenVarsJungle();
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

    private static JungleBounds ValidateGenVarsJungle()
    {
        var bounds = new JungleBounds();

        if (GenVars.jungleMinX > 0 && GenVars.jungleMaxX > GenVars.jungleMinX &&
            GenVars.jungleMaxX < Main.maxTilesX - 200)
        {
            bounds.MinX = GenVars.jungleMinX;
            bounds.MaxX = GenVars.jungleMaxX;
            bounds.SurfaceY = (int)Main.worldSurface;
            bounds.Height = 400;

            if (ValidateJungleArea(bounds.MinX + (bounds.MaxX - bounds.MinX) / 2, bounds.SurfaceY + 50))
            {
                return bounds;
            }
        }

        return new JungleBounds(); // Invalid
    }

    private static JungleBounds ScanForJungleBiome()
    {
        var bounds = new JungleBounds
        {
            MinX = Main.maxTilesX,
            MaxX = 0,
            SurfaceY = (int)Main.worldSurface,
            Height = 400
        };

        var scanStartY = (int)Main.worldSurface;
        var scanEndY = Math.Min((int)Main.worldSurface + 200, Main.maxTilesY - 100);

        for (var x = 200; x < Main.maxTilesX - 200; x += 5)
        {
            for (var y = scanStartY; y < scanEndY; y += 3)
            {
                if (WorldGen.InWorld(x, y) && IsJungleTile(x, y))
                {
                    if (ValidateJungleArea(x, y, 25))
                    {
                        bounds.MinX = Math.Min(bounds.MinX, x);
                        bounds.MaxX = Math.Max(bounds.MaxX, x);
                    }
                }
            }
        }

        if (bounds.IsValid)
        {
            bounds.MinX = Math.Max(bounds.MinX - 50, 200);
            bounds.MaxX = Math.Min(bounds.MaxX + 50, Main.maxTilesX - 200);
        }

        return bounds;
    }

    private static JungleBounds EstimateJungleLocation()
    {
        var jungleOnLeft = Main.dungeonX > Main.maxTilesX / 2;
        var estimatedCenter = jungleOnLeft ? Main.maxTilesX / 4 : Main.maxTilesX * 3 / 4;
        var estimatedWidth = Main.maxTilesX / 5;

        return new JungleBounds
        {
            MinX = Math.Max(estimatedCenter - estimatedWidth / 2, 200),
            MaxX = Math.Min(estimatedCenter + estimatedWidth / 2, Main.maxTilesX - 200),
            SurfaceY = (int)Main.worldSurface,
            Height = 400
        };
    }

    private static bool IsJungleTile(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return false;

        var tile = Main.tile[x, y];
        return tile.HasTile && (tile.TileType == TileID.JungleGrass ||
                               tile.TileType == TileID.Mud ||
                               tile.TileType == TileID.JunglePlants ||
                               tile.TileType == TileID.JungleVines);
    }

    private static bool ValidateJungleArea(int centerX, int centerY, int radius = 25)
    {
        var jungleTiles = 0;
        var totalTiles = 0;

        for (var x = centerX - radius; x <= centerX + radius; x++)
        {
            for (var y = centerY - radius; y <= centerY + radius; y++)
            {
                if (WorldGen.InWorld(x, y))
                {
                    var tile = Main.tile[x, y];
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

        return totalTiles > 0 && (float)jungleTiles / totalTiles > 0.25f;
    }

    public static bool IsDesertOnLeftSideOfJungle(Rectangle jungleRect)
    {
        var desertX = -1;

        if (GenVars.UndergroundDesertLocation.X > 0 && GenVars.UndergroundDesertLocation.X < Main.maxTilesX)
        {
            desertX = GenVars.UndergroundDesertLocation.X;
        }
        else if (GenVars.UndergroundDesertHiveLocation.X > 0 && GenVars.UndergroundDesertHiveLocation.X < Main.maxTilesX)
        {
            desertX = GenVars.UndergroundDesertHiveLocation.X;
        }
        else
        {
            var dungeonOnLeft = Main.dungeonX < Main.maxTilesX / 2;
            desertX = dungeonOnLeft ? Main.maxTilesX / 4 : Main.maxTilesX * 3 / 4;
        }

        if (desertX == -1) return false;

        var jungleCenter = (jungleRect.Left + jungleRect.Right) / 2;
        return desertX < jungleCenter;
    }
}