using Reverie.Content.Tiles.Canopy;
using Reverie.Utilities;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Reverie.Common.WorldGeneration.WoodlandCanopy.CanopyGeneration;
namespace Reverie.Common.WorldGeneration.WoodlandCanopy
{
    public class CanopyFoliagePass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Sprucing up the Woodland Canopy";
        }
    }
}