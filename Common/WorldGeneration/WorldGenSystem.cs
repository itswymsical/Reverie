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
        //WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Generate Ice Biome"], Detour_Tundra);
        WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Shinies"], Detour_Ores);
    }
    private void Detour_Ores(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        var orePass = new OrePass();

        orePass.Apply(progress, configuration);
    }

    /*
    private void Detour_Tundra(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        var tundraPass = new TundraPass();

        tundraPass.Apply(progress, configuration);
    }
    */
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        var tundraIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Generate Ice Biome"));
        var grassIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Spreading Grass"));

        if (grassIndex != 1)
        {
            tasks.Insert(grassIndex + 1, new DecorPass());
        }

        var spawnIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Guide"));

        if (spawnIndex != 1)
        {
            tasks.Insert(spawnIndex + 1, new TaigaPass());
            tasks.Insert(spawnIndex + 2, new TaigaPlantPass());
        }     
    }
}
