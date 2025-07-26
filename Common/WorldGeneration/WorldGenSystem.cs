using Reverie.Common.WorldGeneration.BiomeTypes;
using Reverie.Common.WorldGeneration.Rainforest;
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
        var biomeIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Dungeon"));
        //if (biomeIndex != 1)
        //{
        //    tasks.Insert(biomeIndex + 1, new TaigaPass());
        //    tasks.Insert(biomeIndex + 2, new TaigaGrassPass());
        //    tasks.Insert(biomeIndex + 3, new TaigaPlantPass());
        //}

        //var spawnIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Flowers"));
        //if (spawnIndex != 1)
        //{
        //    tasks.Insert(spawnIndex + 1, new TemperateForestPass());
        //}

        var decorIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Spreading Grass"));
        if (decorIndex != 1)
        {
            tasks.Insert(decorIndex + 1, new DecorPass());
        }

        var canopyIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Dungeon"));
        if (canopyIndex != 1)
        {
            tasks.Insert(canopyIndex + 1, new RainforestBiome());
        }
    }
}
