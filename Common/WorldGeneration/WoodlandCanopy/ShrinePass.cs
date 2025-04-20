using Reverie.Utilities;
using Reverie.Utilities.Extensions;
using StructureHelper;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Reverie.Common.WorldGeneration.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Common.WorldGeneration.WoodlandCanopy
{
    public class ShrinePass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Placing shrines";

            var structureCount = Main.rand.Next(5, 9);

            var canopyMiddle = canopyY;

            var canopyTopQuarter = canopyY - canopyRadiusV * 3 / 4;

            for (var valid = 0; valid < structureCount; valid++)
            {
                var x = Main.rand.Next(canopyX - canopyRadiusH, canopyX + canopyRadiusH + 1);

                var y = Main.rand.Next(canopyTopQuarter, canopyMiddle);

                if (WorldGenUtils.GenerateCanopyShape(x, y, canopyX, canopyY, canopyRadiusH, canopyRadiusV, 0.04f, canopyRadiusH / 4, 100, 15))
                {
                    StructureHelper.API.Generator.GenerateStructure("Structures/CanopyShrine", new Point16(x, y), Instance);
                }
                else
                {
                    valid--;
                }
            }
        }
    }
}