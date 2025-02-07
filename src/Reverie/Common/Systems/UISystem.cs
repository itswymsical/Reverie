using Terraria.Localization;

namespace Reverie.Common.Systems
{
    [Autoload(Side = ModSide.Client)]
    internal class UISystem : ModSystem
    {
        public static UISystem Instance => ModContent.GetInstance<UISystem>();
        public static LocalizedText ExperienceText { get; private set; }

        public override void Load()
        {

            string category = "UI";
            ExperienceText ??= Mod.GetLocalization($"{category}.ExperienceTracker");

        }
    }
}