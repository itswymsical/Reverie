using Reverie.Helpers;
using StructureHelper;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Reverie.Common.Systems.WorldGeneration.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Common.Systems.WorldGeneration.WoodlandCanopy
{
    public class ShrinePass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Placing shrines";

            int structureCount = Main.rand.Next(5, 9);

            int canopyMiddle = canopyY;

            int canopyTopQuarter = canopyY - (canopyRadiusV * 3 / 4);

            for (int valid = 0; valid < structureCount; valid++)
            {
                int x = Main.rand.Next(canopyX - canopyRadiusH, canopyX + canopyRadiusH + 1);

                int y = Main.rand.Next(canopyTopQuarter, canopyMiddle);

                if (Helper.GenerateCanopyShape(x, y, canopyX, canopyY, canopyRadiusH, canopyRadiusV, 0.04f, canopyRadiusH / 4, 100, 15))
                {
                    Generator.GenerateStructure("Structures/CanopyShrine", new Point16(x, y), Reverie.Instance);
                }
                else
                {
                    valid--;
                }
            }
        }
    }
}