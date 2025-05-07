using Terraria.Localization;

namespace Reverie.Common.UI
{
    [Autoload(Side = ModSide.Client)]
    internal class UISystem : ModSystem
    {
        public static UISystem Instance => ModContent.GetInstance<UISystem>();
        public static LocalizedText ExperienceText { get; private set; }

        public override void Load()
        {

            var category = "UI";
            ExperienceText ??= Mod.GetLocalization($"{category}.ExperienceTracker");

        }
    }
}