using Reverie.Common.Systems.WorldGeneration.GenPasses;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Reverie.Common.Systems.WorldGeneration
{
    public class WorldGenerationSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // Find the indexes where the tasks need to be replaced
            int DirtIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Dirt Layer Caves"));
            int RockIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Rock Layer Caves"));
            int TunnelIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Tunnels"));
            int HoleIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Small Holes"));
            //int SpawnIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Spawn Point"));
            int GuideIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Guide"));
            int WaterIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Mud Caves To Grass"));
            int CanopyIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Stalac"));

            if (GuideIndex >= 0)
                tasks[GuideIndex] = new GuideShelterPass("Guide's Refuge", 292f);
            
            if (DirtIndex >= 0) tasks.RemoveAt(DirtIndex);
            if (RockIndex >= 0) tasks.RemoveAt(RockIndex);
            if (TunnelIndex >= 0) tasks.RemoveAt(TunnelIndex);
            if (HoleIndex >= 0) tasks.RemoveAt(HoleIndex);

            if (TunnelIndex >= 0)
                tasks[TunnelIndex] = new CavePass("Caves [Reverie]", 337f); 
            if (HoleIndex >= 0)
                tasks[HoleIndex] = new OrePass("All Vanilla Ores [Reverie]", 337f);
            

            if (WaterIndex >= 0) tasks.Insert(WaterIndex + 1, new LiquidPass("Liquids [Reverie]", 177f));

            if (CanopyIndex != -1)
            {
                // Uncomment the following lines if needed
                // tasks.Insert(CanopyIndex + 1, new CanopyPass("Woodland Canopy", 635f));
                // tasks.Insert(CanopyIndex + 2, new CanopyFoliagePass("Canopy Decor", 280f));
                // tasks.Insert(CanopyIndex + 3, new ReverieTreePass("Reverie Tree", 150f));
                // tasks.Insert(CanopyIndex + 4, new SmoothPass("Smoothing World Again", 100f));
                // tasks.Insert(CanopyIndex + 5, new SanctumPass("Archiver Sanctum", 337f));
            }
        }
    }
}