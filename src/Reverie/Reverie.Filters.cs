using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Reverie;

public sealed partial class Reverie : Mod
{
    /// <summary>
    ///     The name of the screen ripple filter.
    /// </summary>
    public const string SCREEN_RIPPLE_NAME = NAME_PREFIX + "ScreenRipple";
    
    public override void Load()
    {
        base.Load();
        
        if (Main.dedServ)
        {
            return;
        }
        
        Filters.Scene[SCREEN_RIPPLE_NAME] = new Filter(new ScreenShaderData("FilterMiniTower").UseImage("Images/Misc/noise"), EffectPriority.VeryHigh);
        Filters.Scene[SCREEN_RIPPLE_NAME].Load();
    }
}