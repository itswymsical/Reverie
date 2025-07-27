using Reverie.Common.WorldGeneration.BiomeTypes;
using Reverie.Content.Tiles.Canopy;
using Reverie.Content.Tiles.Canopy.Trees;
using Reverie.Content.Tiles.Taiga;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Rainforest;

public class RainforestBiome : TransitionBiome
{
    protected override ushort GetSoilTileType() => (ushort)ModContent.TileType<ClayLoamTile>();
    protected override ushort GetGrassTileType() => (ushort)ModContent.TileType<RainforestGrassTile>();

    public RainforestBiome() : base("[Reverie] Rainforest", 80f, new BiomeConfiguration
    {
        TerrainNoiseFreq = 0.00925f,
        BaseHeightOffset = 6,
        TerrainHeightVariation = 12,
        MinWidth = (int)(Main.maxTilesX * 0.07f),
        MaxWidth = (int)(Main.maxTilesX * 0.09f),
        SurfaceDepth = (int)(Main.maxTilesY * 0.07f)
    })
    { }

    protected override string PassName => "Rainforest";

    protected override int[] GetConflictBiomes() =>
    [
        TileID.Sand, TileID.HardenedSand, // Desert
        TileID.JungleGrass, TileID.Mud,   // Jungle
        TileID.SnowBlock, TileID.IceBlock // Snow
    ];

    protected override bool GetBiomeBounds(out BiomeBounds bounds)
    {
        bounds = default;

        // Place rainforest on side opposite to dungeon
        bool dungeonOnLeft = GenVars.dungeonX < Main.maxTilesX / 2;
        bool placeOnLeft = !dungeonOnLeft; // Opposite side from dungeon

        var rainforestWidth = WorldGen.genRand.Next(_config.MinWidth, _config.MaxWidth);
        int rainforestLeft, rainforestRight;

        if (placeOnLeft)
        {
            rainforestLeft = (int)(Main.maxTilesX * 0.05f);
            rainforestRight = rainforestLeft + rainforestWidth;
            if (rainforestRight > Main.maxTilesX / 2)
            {
                rainforestRight = Main.maxTilesX / 2;
                rainforestWidth = rainforestRight - rainforestLeft;
            }
        }
        else
        {
            rainforestRight = (int)(Main.maxTilesX * 0.95f);
            rainforestLeft = rainforestRight - rainforestWidth;
            if (rainforestLeft < Main.maxTilesX / 2)
            {
                rainforestLeft = Main.maxTilesX / 2;
                rainforestWidth = rainforestRight - rainforestLeft;
            }
        }

        if (rainforestWidth < _config.MinWidth)
            return false;

        bounds = new BiomeBounds
        {
            Left = rainforestLeft,
            Right = rainforestRight,
            Top = (int)Main.worldSurface - 90,
            Bottom = (int)Main.worldSurface + _config.SurfaceDepth
        };

        return bounds.IsValid;
    }

    protected override ushort GetBaseTile(int x, int y, int depthFromSurface, float noiseValue)
    {
        if (depthFromSurface <= _config.SurfaceDepth * 0.15f)
        {
            return (ushort)ModContent.TileType<ClayLoamTile>();
        }
        else if (depthFromSurface <= _config.SurfaceDepth * 0.25f)
        {
            return TileID.ClayBlock;
        }
        else if (depthFromSurface <= _config.SurfaceDepth * 0.5f)
        {
            return TileID.Mud;
        }

        return 0;
    }

    protected override ushort GetTerrainTile(int x, int y, int depthFromSurface)
    {
        if (depthFromSurface <= _config.SurfaceDepth * 0.15f)
        {
            return (ushort)ModContent.TileType<ClayLoamTile>();
        }
        else if (depthFromSurface <= _config.SurfaceDepth * 0.25f)
        {
            return TileID.ClayBlock;
        }
        else if (depthFromSurface <= _config.SurfaceDepth * 0.5f)
        {
            return TileID.Mud;
        }

        return 0;
    }

    protected override bool ShouldSpreadGrass() => true;

    protected override void PopulateBiome(GenerationProgress progress)
    {
        for (int x = _biomeBounds.Left; x < _biomeBounds.Right; x++)
        {
            int surfaceY = _terrainHeights[x - _biomeBounds.Left];
            // Check if surface tile is grass before placing plants
            var surfaceTile = Main.tile[x, surfaceY];
            if (surfaceTile.HasTile && surfaceTile.TileType == GetGrassTileType())
            {
                if (WorldGen.genRand.NextBool(2))
                {
                    WorldGen.PlaceTile(x, surfaceY - 1, ModContent.TileType<CanopyFoliageTile>(), style: Main.rand.Next(18));
                }
                if (WorldGen.genRand.NextBool(28) && HasSpacing(x, surfaceY, 1))
                {
                    WorldGen.PlaceTile(x, surfaceY - 1, ModContent.TileType<CanopyFernTile>());
                }

                if (WorldGen.genRand.NextBool(16) && HasSpacing(x, surfaceY, 1))
                {
                    SmallTanglewoodTree.GrowTanglewoodTree(x, surfaceY - 1);
                }
                if (WorldGen.genRand.NextBool(60) && HasSpacing(x, surfaceY, 1))
                {
                    MediumTanglewoodTree.GrowTanglewoodTree(x, surfaceY - 1);
                }
            }
        }
    }

    private bool HasSpacing(int x, int surfaceY, int minSpacing)
    {
        // Check left and right sides for existing trees
        for (int checkX = x - minSpacing; checkX <= x + minSpacing; checkX++)
        {
            if (checkX == x) continue; // Skip the center position
            if (!WorldGen.InWorld(checkX, surfaceY)) continue;

            // Check if there's a tree at this position
            if (IsTreeAt(checkX, surfaceY))
                return false;
        }

        return true;
    }

    private bool IsTreeAt(int x, int y)
    {
        // Check above surface for tree tiles
        for (int checkY = y - 10; checkY < y; checkY++)
        {
            if (!WorldGen.InWorld(x, checkY)) continue;

            var tile = Main.tile[x, checkY];
            if (tile.HasTile && (tile.TileType == ModContent.TileType<TanglewoodTree>() ||
                                tile.TileType == TileID.Trees || // Vanilla trees
                                tile.TileType == TileID.PalmTree)) // Other tree types you want to check
            {
                return true;
            }
        }

        return false;
    }

    protected override bool ValidatePlacement(BiomeBounds bounds)
    {
        if (bounds.Width < 100)
            return false;

        if (bounds.Left < Main.maxTilesX * 0.25f || bounds.Right > Main.maxTilesX * 0.75f)
            return false;

        //ValidateDesertSeparation(bounds);

        return true;
    }


}
