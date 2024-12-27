using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria;
using Reverie.Utilities;

namespace Reverie.Common.Systems.WorldGeneration.GenPasses
{
    public class LiquidPass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating Liquids";
            // Todo: MAKE LIQUID
            int i = Main.maxTilesX;
            int j = (int)Main.rockLayer + ((int)Main.rockLayer / 4);
            int j2 = Main.UnderworldLayer;

            for (int x = 0; x < i; x++)
            {
                for (int y = (int)Main.worldSurface; y < j2; y++)
                {
                    Tile tile = Main.tile[x, y];
                   
                    if (y <= j)
                    {
                        if (Main.rand.NextBool(10))
                        {
                            WorldGen.PlaceLiquid(x, y, (byte)LiquidID.Water, 225);
                        }
                    }
                    else if (y >= j + (j / 3))
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
}