using SubworldLibrary;
using System.Collections.Generic;

using Terraria.GameContent;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.TestGen;

public class TestSub : Otherworld
{
    public override int Width => 800;
    public override int Height => 1350;
    public override bool ShouldSave => false;
    public override bool NormalUpdates => true;
    public override bool NoPlayerSaving => true; 

    public override List<GenPass> Tasks =>
    [        
        new TestPass(),
    ];

    public override void SetStaticDefaults()
    {
        try
        {
            base.SetStaticDefaults();
            OtherworldTitle = TextureAssets.Logo?.Value;
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>()?.Logger.Error($"Error in TestSub.SetStaticDefaults: {ex.Message}");
            OtherworldTitle = null;
        }
    }

    public override void OnEnter()
    {
        SubworldSystem.hideUnderworld = true;
    }
}