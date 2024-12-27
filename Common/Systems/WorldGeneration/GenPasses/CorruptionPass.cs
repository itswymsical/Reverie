using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Systems.WorldGeneration.GenPasses
{
    public class CorruptionPass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Vitiating Forests";
            //TODO: New Corruption
        }
        private void DoCorruptionBase() { }
        private void DoCorruptionSurface() { }
    }
}
