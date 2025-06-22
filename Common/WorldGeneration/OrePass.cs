using Reverie.Content.Tiles.Misc;
using System.Collections.Generic;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;

public class OrePass : GenPass
{
    private readonly Dictionary<string, OreConfiguration> _oreConfigs;

    public OrePass() : base("Ores", 150f)
    {
        _oreConfigs = new Dictionary<string, OreConfiguration>();
        InitializeAllOreConfigurations();
    }

    private void InitializeAllOreConfigurations()
    {
        _oreConfigs["Copper"] = new OreConfiguration
        {
            TileType = TileID.Copper,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Surface, 0.00006, 3, 6, 2, 6),
                new LayerDistribution(WorldLayer.Dirt, 0.00008, 3, 7, 3, 7),
                new LayerDistribution(WorldLayer.Rock, 0.0002, 4, 9, 4, 8)
            }
        };

        _oreConfigs["Tin"] = new OreConfiguration
        {
            TileType = TileID.Tin,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Surface, 0.00006, 3, 6, 2, 6),
                new LayerDistribution(WorldLayer.Dirt, 0.00008, 3, 7, 3, 7),
                new LayerDistribution(WorldLayer.Rock, 0.0002, 4, 9, 4, 8)
            }
        };

        _oreConfigs["Iron"] = new OreConfiguration
        {
            TileType = TileID.Iron,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Surface, 0.00003, 3, 7, 2, 5),
                new LayerDistribution(WorldLayer.Dirt, 0.00006, 3, 6, 3, 6),
                new LayerDistribution(WorldLayer.Rock, 0.0002, 4, 9, 4, 8)
            }
        };

        _oreConfigs["Lead"] = new OreConfiguration
        {
            TileType = TileID.Lead,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Surface, 0.00003, 3, 7, 2, 5),
                new LayerDistribution(WorldLayer.Dirt, 0.00008, 3, 6, 3, 6),
                new LayerDistribution(WorldLayer.Rock, 0.0002, 4, 9, 4, 8)
            }
        };

        _oreConfigs["Silver"] = new OreConfiguration
        {
            TileType = TileID.Silver,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Dirt, 0.000026, 3, 6, 3, 6),
                new LayerDistribution(WorldLayer.Rock, 0.0001, 4, 9, 4, 8),
                new LayerDistribution(WorldLayer.Sky, 0.00017, 4, 9, 4, 8)
            }
        };

        _oreConfigs["Tungsten"] = new OreConfiguration
        {
            TileType = TileID.Tungsten,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Dirt, 0.000016, 3, 6, 3, 6),
                new LayerDistribution(WorldLayer.Rock, 0.0001, 4, 9, 4, 8),
                new LayerDistribution(WorldLayer.Sky, 0.00017, 4, 9, 4, 8)
            }
        };

        _oreConfigs["Gold"] = new OreConfiguration
        {
            TileType = TileID.Gold,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Rock, 0.00012, 4, 8, 4, 8),
                new LayerDistribution(WorldLayer.Sky, 0.00012, 4, 8, 4, 8)
            }
        };

        _oreConfigs["Platinum"] = new OreConfiguration
        {
            TileType = TileID.Platinum,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Rock, 0.00012, 4, 8, 4, 8),
                new LayerDistribution(WorldLayer.Sky, 0.00012, 4, 8, 4, 8)
            }
        };

        _oreConfigs["Demonite"] = new OreConfiguration
        {
            TileType = TileID.Demonite,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Dirt, 0.0000225, 3, 6, 4, 8),
                new LayerDistribution(WorldLayer.Rock, 0.0000125, 3, 6, 4, 8)
            }
        };

        _oreConfigs["Crimtane"] = new OreConfiguration
        {
            TileType = TileID.Crimtane,
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Dirt, 0.0000225, 3, 6, 4, 8),
                new LayerDistribution(WorldLayer.Rock, 0.0000125, 3, 6, 4, 8)
            }
        };

        _oreConfigs["Lodestone"] = new OreConfiguration
        {
            TileType = ModContent.TileType<LodestoneTile>(),
            Distribution = new List<LayerDistribution>
            {
                new LayerDistribution(WorldLayer.Dirt, 0.00008, 3, 5, 3, 6),
                new LayerDistribution(WorldLayer.Rock, 0.00008, 3, 5, 3, 6)
            }
        };
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating ores...";

        foreach (var orePair in _oreConfigs)
        {
            GenerateOre(orePair.Value);
        }
    }

    private bool IsValidOreLocation(int x, int y, int width, int height)
    {
        int checkRadius = Math.Max(width, height) / 2 + 2;

        for (int checkX = x - checkRadius; checkX <= x + checkRadius; checkX++)
        {
            for (int checkY = y - checkRadius; checkY <= y + checkRadius; checkY++)
            {
                if (WorldGen.InWorld(checkX, checkY))
                {
                    Tile tile = Main.tile[checkX, checkY];
                    if (tile.HasTile && (tile.TileType == TileID.LivingWood ||
                        tile.TileType == TileID.ClayBlock))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private void GenerateOre(OreConfiguration config)
    {
        foreach (var distribution in config.Distribution)
        {
            int count = CalculateOreCount(distribution.Density);

            for (int i = 0; i < count; i++)
            {
                int attempts = 0;
                int maxAttempts = 50; // Prevent infinite loops

                int x, y, width, height;

                do
                {
                    x = WorldGen.genRand.Next(0, Main.maxTilesX);
                    y = GetYPositionForLayer(distribution.Layer);
                    width = WorldGen.genRand.Next(distribution.MinWidth, distribution.MaxWidth + 1);
                    height = WorldGen.genRand.Next(distribution.MinHeight, distribution.MaxHeight + 1);
                    attempts++;
                }
                while (!IsValidOreLocation(x, y, width, height) && attempts < maxAttempts);

                if (attempts < maxAttempts)
                {
                    WorldGen.TileRunner(x, y, width, height, config.TileType);
                }
            }
        }
    }

    private int CalculateOreCount(double density)
    {
        return (int)((double)(Main.maxTilesX * Main.maxTilesY) * density);
    }

    private int GetYPositionForLayer(WorldLayer layer)
    {
        switch (layer)
        {
            case WorldLayer.Sky:
                return WorldGen.genRand.Next(0, (int)GenVars.worldSurfaceLow - 20);
            case WorldLayer.Surface:
                return WorldGen.genRand.Next((int)GenVars.worldSurfaceLow, (int)GenVars.worldSurfaceHigh);
            case WorldLayer.Dirt:
                return WorldGen.genRand.Next((int)GenVars.worldSurfaceHigh, (int)GenVars.rockLayerHigh);
            case WorldLayer.Rock:
                return WorldGen.genRand.Next((int)GenVars.rockLayerLow, Main.maxTilesY);
            default:
                return WorldGen.genRand.Next(0, Main.maxTilesY);
        }
    }
}

public class OreConfiguration
{
    public int TileType { get; set; }
    public List<LayerDistribution> Distribution { get; set; }
}

public class LayerDistribution
{
    public WorldLayer Layer { get; }
    public double Density { get; }
    public int MinWidth { get; }
    public int MaxWidth { get; }
    public int MinHeight { get; }
    public int MaxHeight { get; }

    public LayerDistribution(WorldLayer layer, double density, int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        Layer = layer;
        Density = density;
        MinWidth = minWidth;
        MaxWidth = maxWidth;
        MinHeight = minHeight;
        MaxHeight = maxHeight;
    }
}

public enum WorldLayer
{
    Sky,
    Surface,
    Dirt,
    Rock
}