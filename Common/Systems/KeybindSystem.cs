using Terraria.ModLoader;

namespace Reverie.Common.Systems
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind QuickMagicMirror { get; private set; }
        public static ModKeybind SkillTreeBind { get; private set; }
        public override void Load()
        {
            QuickMagicMirror = KeybindLoader.RegisterKeybind(Mod, "QuickMagicMirror", "Home");
            SkillTreeBind = KeybindLoader.RegisterKeybind(Mod, "ToggleSkillMenu", "O");
        }

        public override void Unload()
        {
            QuickMagicMirror = null;
            SkillTreeBind = null;
        }
    }
}