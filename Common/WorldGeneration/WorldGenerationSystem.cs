using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;
using Reverie.Common.WorldGeneration.Structures;

namespace Reverie.Common.WorldGeneration;

public class WorldGenerationSystem : ModSystem
{
    public override void Load()
    {
        WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Guide"], Detour_Guide);
        WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Tunnels"], Detour_Caves);
    }

    private void Detour_Guide(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        var guideShelter = new GuideHousePass("Guide's Refuge [Reverie]", 85f);

        guideShelter.Apply(progress, configuration);
    }

    private void Detour_Caves(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        var caves = new CavePass("Caves [Reverie]", 450f);

        caves.Apply(progress, configuration);
    }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        tasks.RemoveAll(genPass =>
                     genPass.Name == "Small Holes" ||
                     genPass.Name == "Rock Layer Caves" ||
                     genPass.Name == "Wavy Caves" ||
                     genPass.Name == "Shinies"
                 );

        var WaterIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Mud Caves To Grass"));
        if (WaterIndex >= 0)
        {
            tasks.Insert(WaterIndex, new OrePass("Ore Generation [Reverie]", 200f));
            tasks.Insert(WaterIndex + 1, new LiquidPass("Liquids [Reverie]", 175f));
        }         
    }
}
