using Reverie.Common.WorldGeneration.Structures;
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
        WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Guide"], Detour_Town);
    }

    private void Detour_Tundra(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        var tundraPass = new TundraPass();

        tundraPass.Apply(progress, configuration);
    }

    private void Detour_Town(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        var guidePass = new BeginningTownPass();

        guidePass.Apply(progress, configuration);
    }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        var tundraIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Generate Ice Biome"));
        var livingTreeIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Dirt Rock Wall Runner"));

        var livingTree = tasks.FindIndex(genPass => genPass.Name.Equals("Living Trees"));
        tasks.RemoveAt(livingTree);

        if (tundraIndex >= 0)
        {
            tasks.Insert(tundraIndex + 1, new TaigaPlantPass());
        }

        if (livingTreeIndex >= 0)
        {
            tasks.Insert(tundraIndex + 1, new LivingTreePass());
            tasks.Insert(tundraIndex + 2, new LivingTreeWallPass());
        }

    }
}
