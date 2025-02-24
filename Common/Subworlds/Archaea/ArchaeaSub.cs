using Reverie.Common.Subworlds.Archaea.Generation;
using Reverie.Common.WorldGeneration;

using SubworldLibrary;
using System.Collections.Generic;

using Terraria.GameContent;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Archaea;

public class ArchaeaSubworld : Otherworld
{
    public override int Width => 3600;
    public override int Height => 1700;
    public override bool ShouldSave => false; // for debugging purposes
    public override bool NormalUpdates => true;
    public override bool NoPlayerSaving => true; // for debugging purposes

    public override List<GenPass> Tasks =>
    [
        new DesertPass(),
        new RubblePass(),
        //new CavernPass("[Archaea] Caverns", 250f),
        //new EmberiteCavernsPass("Shelledrake Nest", 250f),
        new SmoothPass("[Reverie] Smooth World", 89f),
        new PlantPass()
    ];

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        OtherworldTitle = TextureAssets.Logo.Value;
    }
    public override void Update()
    {
        Main.time++;
        ManageGameConditions();
    }
    private void ManageGameConditions()
    {
        var player = Main.LocalPlayer;
        if (player.ZoneForest || player.ZoneSkyHeight || player.ZonePurity)
            player.ZoneDesert = true;
        

        if (player.ZoneRockLayerHeight)
            player.ZoneUndergroundDesert = true;
        
        if (Main.raining)
        {
            Main.StopRain();
            Main.raining = false;
            Main.slimeRain = false;
        }

        if (Main.slimeRain)
            Main.slimeRain = false; 

        Main.townNPCCanSpawn[default] = false;
    }

    public override void OnEnter()
    {
        SubworldSystem.hideUnderworld = true;
    }
}