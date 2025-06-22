using Reverie.Common.WorldGeneration.Structures;
using Reverie.Common.WorldGeneration.Taiga;
using Reverie.Common.WorldGeneration.Rainforest;
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
        //var tundraIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Generate Ice Biome"));
        var decorIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Spreading Grass"));

        var canopyIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Full Desert"));

        var spawnIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Guide"));

        if (decorIndex != 1)
        {
            tasks.Insert(decorIndex + 1, new DecorPass());
        }

        if (canopyIndex != 1)
        {
            tasks.Insert(canopyIndex + 1, new RainforestBase());
            tasks.Insert(canopyIndex + 2, new RainforestUndergroundPass());
            tasks.Insert(canopyIndex + 3, new RainforestFoliagePass());
        }  
        
        if (spawnIndex != 1)
        {
            tasks.Insert(spawnIndex + 1, new TaigaPass());
            tasks.Insert(spawnIndex + 2, new TaigaPlantPass());
        }     
    }
}
