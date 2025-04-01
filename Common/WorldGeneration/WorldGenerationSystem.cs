using Reverie.Common.WorldGeneration.Taiga;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration;

public class WorldGenerationSystem : ModSystem
{
    public override void Load()
    {
        WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Generate Ice Biome"], Detour_Tundra);
    }

    private void Detour_Tundra(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        var taiga = new TaigaTerrain();

        taiga.Apply(progress, configuration);
    }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalweight)
    {

        var snowindex = tasks.FindIndex(genpass => genpass.Name.Equals("Generate Ice Biome"));
        if (snowindex >= 0)
        {
            tasks.Insert(snowindex + 1, new TaigaGrassGenPass());
        }
    }
}
