using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader;

namespace Reverie.Core
{
    public class ILEdits : ModSystem
    {
        public override void OnModLoad()
        {
            IL_UIWorldCreation.AddWorldSizeOptions += SmallWorldMessage;
        }
        private static void SmallWorldMessage(ILContext il)
        {
            var c = new ILCursor(il);
            var c2 = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdstr("UI.WorldDescriptionSizeSmall")))
            {
                Reverie.Instance.Logger.WarnFormat("Edit Small World Desc", "Could not match string \"UI.WorldDescriptionSizeSmall\".");
                return;
            }
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldstr, "[c/FF0000:Small worlds are NOT supported by Reverie.]");
            if (!c2.TryGotoNext(MoveType.After, x => x.MatchLdstr("UI.WorldDescriptionSizeLarge")))
            {
                Reverie.Instance.Logger.WarnFormat("Edit Large World Desc", "Could not match string \"UI.WorldDescriptionSizeLarge\".");
                return;
            }
            c2.Emit(OpCodes.Pop);
            c2.Emit(OpCodes.Ldstr, "[c/ff8e2b:Cave generation may take substantially longer on large worlds!]");
        }

    }
}
