using Reverie.Common.Systems.WorldGeneration.GenPasses;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.WorldBuilding;

namespace Reverie.Common.Systems.Subworlds.Archaea
{
    public class ArchaeaSubworld : Otherworld
    {
        public override int Width => 3600;
        public override int Height => 1700;
        public override bool ShouldSave => false;
        public override bool NormalUpdates => true;
        public override bool NoPlayerSaving => true;
        public override List<GenPass> Tasks =>
        [
            new DesertPass("[Archaea] Desert", 153f),
            new CavernPass("[Archaea] Caverns", 250f),
            new EmberiteCavernsPass("Shelledrake Nest", 250f),
            new SmoothPass("Smooth World - Reverie", 89f),
            new PlantPass("[Archaea] Plants", 77f)
        ];
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            OtherworldTitle = TextureAssets.Logo2.Value;
        }
        public override void Update()
        {
            Main.time += 1;
            Player player = Main.LocalPlayer;
            if (player.ZoneForest || player.ZoneSkyHeight || player.ZonePurity)
            {
                player.ZoneDesert = true;
            }
            if (player.ZoneRockLayerHeight)
            {
                player.ZoneUndergroundDesert = true;
            }
            if (Main.raining)
            {
                Main.StopRain();
                Main.raining = false;
                Main.slimeRain = false;
            }
            if (Main.slimeRain)
            {
                Main.slimeRain = false;
            }

            Main.townNPCCanSpawn[default] = false;
        }
        public override void OnEnter()
        {
            SubworldSystem.hideUnderworld = true;
        }
    }
}