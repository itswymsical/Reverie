using MonoMod.Cil;

namespace Reverie.Common.Systems;

public class ChatButtonSystem : ModSystem
{
    private static bool initialized;
    private delegate void orig_SetChatButtons(NPC npc, ref string button, ref string button2);
    private delegate void hook_SetChatButtons(orig_SetChatButtons orig, NPC npc, ref string button, ref string button2);

    public override void Load()
    {
        if (!initialized)
        {
            IL_Main.DrawInterface_36_Cursor += IL_Main_DrawInterface_36_Cursor;
            initialized = true;
        }
    }

    private void IL_Main_DrawInterface_36_Cursor(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(i => i.MatchLdloc(7),
                          i => i.MatchLdloc(8),
                          i => i.MatchCallvirt<NPC>("SetChatButtons")))
        {
            Instance.Logger.Warn("Failed to find chat button IL target");
            return;
        }

        c.EmitDelegate<Action<NPC, string, string>>((npc, button, button2) =>
        {
            if (npc.ModNPC != null)
            {
                return;
            }
        });
    }
}

public interface IWorldNPCChat
{
    void ModifyVanillaChatButtons(NPC npc, ref string button, ref string button2);
    void OnChatButtonClicked(NPC npc, bool firstButton);
}