using Reverie.Common.WorldGeneration.Taiga;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;

public class WorldGenSystem : ModSystem
{
    public override void Load()
    {
        WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Generate Ice Biome"], Detour_Tundra);
    }

    private void Detour_Tundra(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        var tundraPass = new TundraPass();

        tundraPass.Apply(progress, configuration);
    }
    //public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    //{
    //    var tundraIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Generate Ice Biome"));
    //    if (tundraIndex >= 0)
    //    {
    //        tasks.Insert(tundraIndex + 1, new TaigaPass());
    //    }
    //}
}
