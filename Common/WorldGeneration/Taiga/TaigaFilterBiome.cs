using Reverie.Common.WorldGeneration.BiomeTypes;
using Reverie.Content.Tiles.Canopy.Trees;
using Reverie.Content.Tiles.Canopy;
using Reverie.Content.Tiles.Taiga;
using Reverie.lib;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TaigaFilterBiome : FilterBiome
{
    public TaigaFilterBiome() : base("[Reverie] Taiga Biome", 247.43f, new BiomeConfiguration
    {
        MinWidth = 100,
        MaxWidth = 300,
        SurfaceDepth = 64,
        BiomeSeparation = 30,
        TerrainNoiseFreq = 0.0095f,
        TerrainHeightVariation = 30,
        BaseNoiseFreq = 0.015f
    })
    { }

    protected override string PassName => "Taiga";

    protected override bool GetBiomeBounds(out BiomeBounds bounds)
    {
        bounds = default;

        if (!CanFitBetweenBiomes())
        {
            return false; // Skip generation
        }

        if (TryPlaceBetweenBiomes(out var taigaLeft, out var taigaRight))
        {
            bounds = new BiomeBounds
            {
                Left = taigaLeft,
                Right = taigaRight,
                Top = (int)Main.worldSurface - 60,
                Bottom = (int)Main.worldSurface + _config.SurfaceDepth
            };
            return bounds.IsValid;
        }

        if (FindSuitableLocation(out taigaLeft, out taigaRight))
        {
            bounds = new BiomeBounds
            {
                Left = taigaLeft,
                Right = taigaRight,
                Top = (int)Main.worldSurface - 60,
                Bottom = (int)Main.worldSurface + _config.SurfaceDepth
            };
            return bounds.IsValid;
        }

        return false;
    }

    protected override ushort GetBaseTile(int x, int y, int depthFromSurface, float noiseValue)
    {
        return (ushort)ModContent.TileType<PeatTile>();
    }

    protected override ushort GetTerrainTile(int x, int y, int depthFromSurface)
    {
        return (ushort)ModContent.TileType<PeatTile>();
    }

    protected override void PopulateBiome(GenerationProgress progress)
    {
        base.PopulateBiome(progress);

        for (int x = _biomeBounds.Left; x < _biomeBounds.Right; x++)
        {
            for (int y = _biomeBounds.Top - 10; y < _biomeBounds.Top + _config.SurfaceDepth + 30; y++)
            {
                if (!WorldGen.InWorld(x, y)) continue;

                var tile = Main.tile[x, y];
                if (tile.HasTile && tile.TileType == (ushort)ModContent.TileType<PeatTile>())
                {
                    tile.WallType = WallID.DirtUnsafe2;
                }
            }
        }
    }

    private bool CanFitBetweenBiomes()
    {
        var dungeonX = GenVars.dungeonX;
        var snowLeft = GenVars.snowOriginLeft;
        var snowRight = GenVars.snowOriginRight;

        // Calculate distance between dungeon and closest snow edge
        int distanceToSnow;
        if (GenVars.dungeonSide < 0) // Dungeon on left
        {
            distanceToSnow = Math.Abs(snowLeft - dungeonX);
        }
        else // Dungeon on right
        {
            distanceToSnow = Math.Abs(dungeonX - snowRight);
        }

        // Need at least 400 tiles between dungeon and snow for taiga to fit
        var minRequiredDistance = 400;
        return distanceToSnow >= minRequiredDistance;
    }

    private bool TryPlaceBetweenBiomes(out int taigaLeft, out int taigaRight)
    {
        var dungeonX = GenVars.dungeonX;
        var snowLeft = GenVars.snowOriginLeft;
        var snowRight = GenVars.snowOriginRight;

        var bufferZone = 80;
        var minTaigaWidth = 150;

        // Calculate proper snow edge based on dungeon position
        int snowEdge;
        if (GenVars.dungeonSide < 0) // Dungeon on left side of world
        {
            snowEdge = snowLeft; // Use left edge of snow (closest to dungeon)
        }
        else // Dungeon on right side of world
        {
            snowEdge = snowRight; // Use right edge of snow (closest to dungeon)
        }

        var availableSpace = Math.Abs(dungeonX - snowEdge);

        if (availableSpace < minTaigaWidth + bufferZone * 2)
        {
            taigaLeft = taigaRight = 0;
            return false;
        }

        // Place taiga between dungeon and snow
        if (GenVars.dungeonSide < 0) // Dungeon on left
        {
            taigaLeft = dungeonX + bufferZone;
            taigaRight = snowEdge - bufferZone;
        }
        else // Dungeon on right
        {
            taigaLeft = snowEdge + bufferZone;
            taigaRight = dungeonX - bufferZone;
        }

        // Clamp to world bounds
        taigaLeft = Math.Max(taigaLeft, 100);
        taigaRight = Math.Min(taigaRight, Main.maxTilesX - 100);

        return taigaRight > taigaLeft + minTaigaWidth;
    }

    private bool FindSuitableLocation(out int taigaLeft, out int taigaRight)
    {
        var taigaWidth = WorldGen.genRand.Next(_config.MinWidth, _config.MaxWidth);

        for (var startX = 200; startX < Main.maxTilesX - taigaWidth - 200; startX += 50)
        {
            if (IsNearExistingBiome(startX, taigaWidth) || !IsSuitableTerrain(startX, taigaWidth))
                continue;

            taigaLeft = startX;
            taigaRight = startX + taigaWidth;
            return true;
        }

        taigaLeft = taigaRight = 0;
        return false;
    }

    private bool IsNearExistingBiome(int startX, int width)
    {
        var surfaceY = (int)Main.worldSurface;
        var samplePoints = 10;

        for (var i = 0; i < samplePoints; i++)
        {
            var x = startX + width * i / samplePoints;
            if (!WorldGen.InWorld(x, surfaceY)) continue;

            var tile = Main.tile[x, surfaceY];

            if (tile.TileType == TileID.Sand || tile.TileType == TileID.HardenedSand ||
                tile.TileType == TileID.JungleGrass || tile.TileType == TileID.Mud ||
                tile.TileType == TileID.SnowBlock || tile.TileType == TileID.IceBlock)
            {
                return true;
            }
        }

        // Check snow proximity
        var snowDistance = Math.Min(Math.Abs(startX - GenVars.snowOriginLeft),
                                    Math.Abs(startX + width - GenVars.snowOriginRight));
        return snowDistance < 100;
    }

    private bool IsSuitableTerrain(int startX, int width)
    {
        var surfaceY = (int)Main.worldSurface;
        var suitableCount = 0;
        var totalSamples = 20;

        for (var i = 0; i < totalSamples; i++)
        {
            var x = startX + width * i / totalSamples;
            if (!WorldGen.InWorld(x, surfaceY)) continue;

            var tile = Main.tile[x, surfaceY];

            if (tile.TileType == TileID.Dirt || tile.TileType == TileID.Grass ||
                tile.TileType == TileID.Stone || tile.TileType == TileID.ClayBlock ||
                tile.TileType == TileID.Ebonsand || tile.TileType == TileID.Ebonstone ||
                tile.TileType == TileID.CorruptGrass || tile.TileType == TileID.Crimsand ||
                tile.TileType == TileID.Crimstone || tile.TileType == TileID.CrimsonGrass)
            {
                suitableCount++;
            }
        }

        return (double)suitableCount / totalSamples >= 0.7;
    }

}