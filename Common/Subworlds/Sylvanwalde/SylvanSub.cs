using Reverie.Common.Subworlds.Archaea.Generation;
using Reverie.Common.Subworlds.Sylvanwalde.Generation;
using Reverie.Common.Subworlds.Sylvanwalde.Generation.WoodlandCanopy;
using Reverie.Common.WorldGeneration;
using SubworldLibrary;
using System.Collections.Generic;

using Terraria.GameContent;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Sylvanwalde;

public class SylvanSub : Otherworld
{
    public override int Width => 4200;
    public override int Height => 1680;
    public override bool ShouldSave => false; // for debugging purposes
    public override bool NormalUpdates => true;
    public override bool NoPlayerSaving => true; // for debugging purposes

    public override List<GenPass> Tasks =>
    [
        new SylvanTerrainPass(),

        //new CanopyPass(),
        //new ReverieTreePass(),
        //new CanopyFoliagePass(),
        //new ShrinePass(),
        new SylvanGrassPass(),
        new SylvanPlantPass(),
        new SmoothPass(),
    ];

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        OtherworldTitle = TextureAssets.Logo.Value;
    }
    public override void Update()
    {
        ManageGameConditions();
    }
    private void ManageGameConditions()
    {
        var player = Main.LocalPlayer;

        Main.townNPCCanSpawn[default] = false;
    }

    public override void OnEnter()
    {
        SubworldSystem.hideUnderworld = true;
    }
}