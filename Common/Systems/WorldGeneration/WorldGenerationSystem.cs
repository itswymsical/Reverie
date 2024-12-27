using Reverie.Common.Systems.Subworlds.Archaea;
using Reverie.Common.Systems.WorldGeneration.GenPasses;
using Reverie.Common.Systems.WorldGeneration.WoodlandCanopy;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Reverie.Common.Systems.WorldGeneration
{
    public class WorldGenerationSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int DirtIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Dirt Layer Caves"));
            int RockIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Rock Layer Caves"));
            int TunnelIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Tunnels"));
            int HoleIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Small Holes"));
      
            tasks.RemoveAt(TunnelIndex);
            tasks.Insert(TunnelIndex, new CavePass("Caves [Reverie]", 337f));
            tasks.RemoveAt(RockIndex);
            tasks.RemoveAt(DirtIndex);

            tasks.RemoveAt(HoleIndex);
            tasks.Insert(HoleIndex, new OrePass("All Vanilla Ores [Reverie]", 337f));

            int Water = tasks.FindIndex(genpass => genpass.Name.Equals("Mud Caves To Grass"));
            tasks.Insert(Water + 1, new LiquidPass("Liquids [Reverie]", 177f));


            int CanopyIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Stalac"));
            if (CanopyIndex != 1)
            {
                tasks.Insert(CanopyIndex + 1, new CanopyPass("Woodland Canopy", 635f));
                tasks.Insert(CanopyIndex + 2, new CanopyFoliagePass("Canopy Decor", 280f));
                tasks.Insert(CanopyIndex + 3, new ReverieTreePass("Reverie Tree", 150f));
                tasks.Insert(CanopyIndex + 4, new SmoothPass("Smoothing World Again", 100f));
                //tasks.Insert(CanopyIndex + 5, new ShrinePass("Canopy Shrines", 183f));
                tasks.Insert(CanopyIndex + 5, new SanctumPass("Archiver Sanctum", 337f));
            }
        }
    }
}