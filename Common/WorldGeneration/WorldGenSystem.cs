using Reverie.Common.WorldGeneration.FilterBiomeSystem;
//using Reverie.Common.WorldGeneration.Corruption;
using Reverie.Common.WorldGeneration.Rainforest;
using Reverie.Common.WorldGeneration.Structures;
using Reverie.Common.WorldGeneration.Taiga;
using Reverie.Common.WorldGeneration.TemperateForest;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;

public class WorldGenSystem : ModSystem
{
    public override void Load()
    {
        WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Shinies"], Detour_Ores);
    }

    private void Detour_Ores(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        var orePass = new OrePass();

        orePass.Apply(progress, configuration);
    }


    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {

        var spawnIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Flowers"));
        if (spawnIndex != 1)
        {
            tasks.Insert(spawnIndex + 1, new TemperateForestPass());
        }

        var decorIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Spreading Grass"));
        if (decorIndex != 1)
        {
            tasks.Insert(decorIndex + 1, new DecorPass());
        }
        var structIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Mushroom Patches"));
        if (structIndex != 1)
        {
            tasks.Insert(structIndex + 1, new ArgieHousePass());
        }

        var biomeIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Dungeon"));
        if (biomeIndex != 1)
        {
           // tasks.Insert(biomeIndex + 1, new RainforestBiome());
            tasks.Insert(biomeIndex + 2, new TaigaPass());
            tasks.Insert(biomeIndex + 3, new TaigaDecorPass());
        }
    }
}
