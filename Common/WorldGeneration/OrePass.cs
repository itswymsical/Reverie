using Reverie.Content.Tiles;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Reverie.Utilities.WorldGenUtils;


namespace Reverie.Common.WorldGeneration;
public class OrePass(string name, double loadWeight) : GenPass(name, loadWeight)
{
    #region Fields
    private readonly int caveLayer = (int)(Main.rockLayer + Main.rockLayer * 0.35);
    private readonly int underworldLayer = Main.UnderworldLayer;

    private static readonly OreConfig[] SurfaceOre =
{
        new(TileID.Copper, 7, 11, 5),
        new(TileID.Tin, 7, 11, 5),
        new(TileID.Iron, 7, 9, 5),
        new(TileID.Lead, 7, 9, 5),

        new(TileID.Topaz, 2, 3, 5),
        new(TileID.Amethyst, 2, 3, 5),
    };

    private static readonly OreConfig[] CaveOre =
{
        new(TileID.Copper, 7, 11, 5),
        new(TileID.Tin, 7, 11, 5),

        new(TileID.Iron, 7, 9, 5),
        new(TileID.Lead, 7, 9, 5),

        new(TileID.Silver, 6, 9, 5),
        new(TileID.Tungsten, 6, 9, 5),

        new(TileID.Gold, 5, 9, 5),
        new(TileID.Platinum, 4, 9, 4),

        new(TileID.Diamond, 2, 3, 5),
        new(TileID.Ruby, 2, 3, 5),
        new(TileID.Sapphire, 2, 3, 5),
        new(TileID.Emerald, 2, 3, 5),
        new(TileID.Topaz, 2, 3, 5),
        new(TileID.Amethyst, 2, 3, 5),

        new((ushort)ModContent.TileType<LodestoneTile>(), 6, 9, 5)
    };


    private static void GenerateOreVein(int x, int y, OreConfig ore)
    {
        WorldGen.TileRunner(
            x, y,
            WorldGen.genRand.Next(ore.MinSize, ore.MaxSize),
            WorldGen.genRand.Next(ore.MinHeight, ore.MinHeight + 4),
            ore.TileType
        );
    }
    private static readonly HashSet<ushort> ValidTiles =
    [
        TileID.Dirt,
        TileID.Mud,
        TileID.Stone,
        TileID.HardenedSand,
        TileID.Sandstone,
        TileID.IceBlock,
        TileID.SnowBlock,
        TileID.Marble,
        TileID.Granite
    ];
    #endregion

    #region Tile Validation
    private static bool IsValidTile(int x, int y)
    {
        var tile = Main.tile[x, y];
        return tile.HasTile && ValidTiles.Contains(tile.TileType);
    }
    #endregion

    #region Ore Generation
    private void GenerateOres(GenerationProgress progress)
    {
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = caveLayer; y < underworldLayer; y++)
            {
                float completionProgress = (float)x / Main.maxTilesX;

                if (!IsValidTile(x, y)) continue;

                if (Main.rand.NextBool(365))
                {
                    var ores = y < (int)Main.rockLayer ? SurfaceOre : CaveOre;
                    GenerateOreVein(x, y, ores[WorldGen.genRand.Next(ores.Length)]);
                }
    
                progress.Set(completionProgress);
            }
        }
    }
    #endregion

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating ore patches";
        GenerateOres(progress);
    }
}