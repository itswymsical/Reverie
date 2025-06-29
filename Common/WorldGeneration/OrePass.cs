using Reverie.Content.Tiles.Misc;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;
public class OrePass : GenPass
{
    public OrePass() : base("Ores", 150f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating ores...";

        GenerateSurfaceOres();
        GenerateUndergroundOres();
        GenerateCavernOres();
        GenerateSpecialBiomeOres();
    }

    private void GenerateSurfaceOres()
    {
        var surfaceStart = (int)GenVars.worldSurfaceLow;
        var surfaceEnd = (int)GenVars.worldSurfaceHigh;

        GenerateOreByBlockType(1.3E-05, surfaceStart, surfaceEnd, 3, 6, 2, 6,
            TileID.Copper, TileID.Stone, TileID.Dirt);
        GenerateOreByBlockType(1.3E-05, surfaceStart, surfaceEnd, 3, 6, 2, 6,
            TileID.Tin, TileID.Stone, TileID.Dirt);
        GenerateOreByBlockType(1.2E-05, surfaceStart, surfaceEnd, 3, 7, 2, 5,
            TileID.Iron, TileID.Stone, TileID.Dirt);

        GenerateOreByBlockType(2.4E-05, surfaceStart, surfaceEnd, 3, 6, 2, 6,
            TileID.Tin, TileID.SnowBlock, TileID.IceBlock);
        GenerateOreByBlockType(1.8E-05, surfaceStart, surfaceEnd, 3, 7, 2, 5,
            TileID.Lead, TileID.SnowBlock, TileID.IceBlock);
        GenerateOreByBlockType(1.2E-05, surfaceStart, surfaceEnd, 3, 7, 2, 5,
            TileID.Iron, TileID.SnowBlock, TileID.IceBlock);

        GenerateOreByBlockType(2.4E-05, surfaceStart, surfaceEnd, 3, 6, 2, 6,
            TileID.Copper, TileID.Sand, TileID.Sandstone);
        GenerateOreByBlockType(1.8E-05, surfaceStart, surfaceEnd, 3, 7, 2, 5,
            TileID.Lead, TileID.Sand, TileID.Sandstone);
        GenerateOreByBlockType(1.2E-05, surfaceStart, surfaceEnd, 3, 7, 2, 5,
            TileID.Iron, TileID.Sand, TileID.Sandstone);

        GenerateOreByBlockType(1.8E-05, surfaceStart, surfaceEnd, 3, 6, 2, 6,
            TileID.Copper, TileID.Sand, TileID.Stone);
        GenerateOreByBlockType(1.8E-05, surfaceStart, surfaceEnd, 3, 6, 2, 6,
            TileID.Tin, TileID.Sand, TileID.Stone);
        GenerateOreByBlockType(1.2E-05, surfaceStart, surfaceEnd, 3, 7, 2, 5,
            TileID.Lead, TileID.Sand, TileID.Stone);
    }

    private void GenerateUndergroundOres()
    {
        var undergroundStart = (int)GenVars.worldSurfaceHigh;
        var undergroundEnd = (int)GenVars.rockLayerHigh;

        GenerateOreByBlockType(3.7E-05, undergroundStart, undergroundEnd, 3, 7, 3, 7,
            TileID.Copper, TileID.Stone);
        GenerateOreByBlockType(3.7E-05, undergroundStart, undergroundEnd, 3, 7, 3, 7,
            TileID.Tin, TileID.Stone);
        GenerateOreByBlockType(3.1E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Iron, TileID.Stone);
        GenerateOreByBlockType(2.4E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Lead, TileID.Stone);

        GenerateOreByBlockType(1.8E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Tungsten, TileID.SnowBlock, TileID.IceBlock, TileID.Stone);
        GenerateOreByBlockType(1E-05, undergroundStart, undergroundEnd, 4, 8, 4, 8,
            TileID.Platinum, TileID.SnowBlock, TileID.IceBlock, TileID.Stone);
        GenerateOreByBlockType(1.2E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Silver, TileID.SnowBlock, TileID.IceBlock, TileID.Stone);

        GenerateOreByBlockType(1.8E-05, undergroundStart, undergroundEnd, 4, 8, 4, 8,
            TileID.Gold, TileID.Sand, TileID.Sandstone, TileID.HardenedSand, TileID.Stone);
        GenerateOreByBlockType(1.2E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Tungsten, TileID.Sand, TileID.Sandstone, TileID.HardenedSand, TileID.Stone);
        GenerateOreByBlockType(1E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Silver, TileID.Sand, TileID.Sandstone, TileID.HardenedSand, TileID.Stone);

        GenerateOreByBlockType(1.8E-05, undergroundStart, undergroundEnd, 4, 8, 4, 8,
            TileID.Platinum, TileID.Mud, TileID.Stone);
        GenerateOreByBlockType(1.2E-05, undergroundStart, undergroundEnd, 4, 8, 4, 8,
            TileID.Gold, TileID.Mud, TileID.Stone);
        GenerateOreByBlockType(1E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Silver, TileID.Mud, TileID.Stone);

        GenerateOreByBlockType(1.8E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Silver, TileID.Stone);
        GenerateOreByBlockType(1.2E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Tungsten, TileID.Stone);
        GenerateOreByBlockType(1E-05, undergroundStart, undergroundEnd, 4, 8, 4, 8,
            TileID.Gold, TileID.Stone);

        GenerateOreByBlockType(2.8E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            ModContent.TileType<LodestoneTile>(), [TileID.Granite, TileID.Marble, TileID.Stone]);
    }

    private void GenerateCavernOres()
    {
        var cavernStart = (int)GenVars.rockLayerHigh;
        var cavernEnd = Main.maxTilesY;

        GenerateOreByBlockType(7.3E-05, cavernStart, cavernEnd, 4, 9, 4, 8,
            TileID.Copper, TileID.Stone);
        GenerateOreByBlockType(7.3E-05, cavernStart, cavernEnd, 4, 9, 4, 8,
            TileID.Tin, TileID.Stone);
        GenerateOreByBlockType(7.3E-05, cavernStart, cavernEnd, 4, 9, 4, 8,
            TileID.Iron, TileID.Stone);
        GenerateOreByBlockType(7.3E-05, cavernStart, cavernEnd, 4, 9, 4, 8,
            TileID.Lead, TileID.Stone);

        GenerateOreByBlockType(4.9E-05, cavernStart, cavernEnd, 4, 9, 4, 8,
            TileID.Silver, TileID.Stone);
        GenerateOreByBlockType(3.7E-05, cavernStart, cavernEnd, 4, 9, 4, 8,
            TileID.Tungsten, TileID.Stone);
        GenerateOreByBlockType(3.7E-05, cavernStart, cavernEnd, 4, 8, 4, 8,
            TileID.Gold, TileID.Stone);
        GenerateOreByBlockType(3.7E-05, cavernStart, cavernEnd, 4, 8, 4, 8,
            TileID.Platinum, TileID.Stone);

        GenerateOreByBlockType(4.9E-05, 0, (int)GenVars.worldSurfaceLow - 20, 4, 9, 4, 8,
            TileID.Silver, TileID.Stone);
        GenerateOreByBlockType(4.9E-05, 0, (int)GenVars.worldSurfaceLow - 20, 4, 9, 4, 8,
            TileID.Tungsten, TileID.Stone);
        GenerateOreByBlockType(3.7E-05, 0, (int)GenVars.worldSurfaceLow - 20, 4, 8, 4, 8,
            TileID.Gold, TileID.Stone);
        GenerateOreByBlockType(3.7E-05, 0, (int)GenVars.worldSurfaceLow - 20, 4, 8, 4, 8,
            TileID.Platinum, TileID.Stone);

        GenerateOreByBlockType(1E-05, cavernStart, cavernEnd, 3, 6, 4, 8,
            TileID.Demonite, TileID.Ebonstone, TileID.Stone);
        GenerateOreByBlockType(1.2E-05, cavernStart, cavernEnd, 3, 6, 4, 8,
            TileID.Tungsten, TileID.Ebonstone, TileID.Stone);
        GenerateOreByBlockType(6E-06, cavernStart, cavernEnd, 3, 6, 4, 8,
            TileID.Lead, TileID.Ebonstone, TileID.Stone);

        GenerateOreByBlockType(1E-05, cavernStart, cavernEnd, 3, 6, 4, 8,
            TileID.Crimtane, TileID.Crimstone, TileID.Stone);
        GenerateOreByBlockType(1.2E-05, cavernStart, cavernEnd, 3, 6, 4, 8,
            TileID.Gold, TileID.Crimstone, TileID.Stone);
        GenerateOreByBlockType(6E-06, cavernStart, cavernEnd, 3, 6, 4, 8,
            TileID.Iron, TileID.Crimstone, TileID.Stone);

        GenerateOreByBlockType(1.8E-05, Main.maxTilesY - 200, Main.maxTilesY, 6, 12, 6, 12,
            TileID.Hellstone, TileID.Ash, TileID.Stone);


        GenerateOreByBlockType(2.8E-05, cavernStart, cavernEnd, 3, 6, 3, 6,
            ModContent.TileType<LodestoneTile>(), [TileID.Granite, TileID.Marble, TileID.Stone]);
    }

    private void GenerateSpecialBiomeOres()
    {
        var undergroundStart = (int)GenVars.worldSurfaceHigh;
        var undergroundEnd = (int)GenVars.rockLayerHigh;

        GenerateOreByBlockType(3.7E-05, undergroundStart, undergroundEnd, 3, 5, 3, 6,
            ModContent.TileType<LodestoneTile>(), TileID.Marble);
        GenerateOreByBlockType(3.7E-05, undergroundStart, undergroundEnd, 3, 5, 3, 6,
            ModContent.TileType<LodestoneTile>(), TileID.Granite);

        GenerateOreByBlockType(1E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Silver, TileID.Marble);
        GenerateOreByBlockType(1E-05, undergroundStart, undergroundEnd, 3, 6, 3, 6,
            TileID.Gold, TileID.Granite);
    }

    private void GenerateOreByBlockType(double density, int minY, int maxY, int minWidth, int maxWidth,
        int minHeight, int maxHeight, int oreType, params int[] validBlocks)
    {
        var count = (int)(Main.maxTilesX * Main.maxTilesY * density);

        for (var i = 0; i < count; i++)
        {
            var attempts = 0;
            var maxAttempts = 50;

            do
            {
                var x = WorldGen.genRand.Next(0, Main.maxTilesX);
                var y = WorldGen.genRand.Next(minY, maxY);

                if (WorldGen.InWorld(x, y) && IsValidBlockType(x, y, validBlocks))
                {
                    var width = WorldGen.genRand.Next(minWidth, maxWidth + 1);
                    var height = WorldGen.genRand.Next(minHeight, maxHeight + 1);

                    WorldGen.TileRunner(x, y, width, height, oreType);
                    break;
                }
                attempts++;
            }
            while (attempts < maxAttempts);
        }
    }

    private bool IsValidBlockType(int x, int y, params int[] validBlocks)
    {
        if (!WorldGen.InWorld(x, y)) return false;

        var tile = Main.tile[x, y];
        if (!tile.HasTile) return false;

        foreach (var blockType in validBlocks)
        {
            if (tile.TileType == blockType)
                return true;
        }
        return false;
    }
}