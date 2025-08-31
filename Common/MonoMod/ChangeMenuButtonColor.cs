// Credits: https://github.com/ZenTheMod/WizenkleBoss/blob/main/Common/MenuStyles/InkPondMenu.cs#L27
// By ZenTheMod
using MonoMod.Cil;
using Reverie.Content.Menus;

namespace Reverie.Common.MonoMod;

public class ModifyMenuButtonsSystem : ModSystem
{
    public override void Load()
    {
        IL_Main.DrawMenu += ModifyButtons;
    }

    public override void Unload()
    {
        IL_Main.DrawMenu -= ModifyButtons;
    }

    private void ModifyButtons(ILContext il)
    {
        ILCursor c = new(il);

        if (!c.TryGotoNext(MoveType.After,
            i => i.MatchLdloca(2),
            i => i.MatchLdloc1(),
            i => i.MatchLdloc1(),
            i => i.MatchLdloc1(),
            i => i.MatchLdcI4(255),
            i => i.MatchCall<Color>(".ctor")))
        {
            Mod.Logger.Warn("Could not change menu button colors, failed to match to after local 2 \"color\" was created.");

            throw new ILPatchFailureException(Mod, il, null);
        }

        c.EmitLdloca(2);

        c.EmitDelegate((ref Color color) =>
        {
            if (!IllustriousMenu.InMenu)
                return;
            color = new Color(77, 216, 203) * 1.1f;
        });
    }
}