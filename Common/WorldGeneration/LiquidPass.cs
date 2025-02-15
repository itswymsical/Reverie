using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;

public class LiquidPass(string name, float loadWeight) : GenPass(name, loadWeight)
{
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating Liquids";
        var i = Main.maxTilesX;
        var j = (int)Main.rockLayer + (int)Main.rockLayer / 4;
        var j2 = Main.UnderworldLayer;

        for (var x = 0; x < i; x++)
        {
            for (var y = (int)Main.worldSurface; y < j2; y++)
            {
                if (y <= j)
                {
                    if (Main.rand.NextBool(10))
                    {
                        WorldGen.PlaceLiquid(x, y, (byte)LiquidID.Water, 225);
                    }
                }

                else if (y >= j + j / 3)
                {
                    if (Main.rand.NextBool(17))
                    {
                        WorldGen.PlaceLiquid(x, y, (byte)LiquidID.Lava, 225);
                    }
                }
            }
        }
    }
}