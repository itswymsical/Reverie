using Microsoft.Xna.Framework;
using Reverie.Common.Players;
using Reverie.Common.UI;
using Reverie.Common.UI.ClassUI;
using Reverie.Common.UI.MissionUI;
using Reverie.Common.UI.SkillTree;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace Reverie.Common.Systems
{
    [Autoload(Side = ModSide.Client)]
    internal class ReverieUISystem : ModSystem
    {
        public static ReverieUISystem Instance => ModContent.GetInstance<ReverieUISystem>();

        internal SkillTreeManager excavationUI;
        private UserInterface ExcavationInterface;

        internal ClassSelectionUI classUI;
        internal UserInterface ClassInterface;

        internal MirrorNetUI mirrorUI;
        private UserInterface MagicMirrorInterface;
        public static LocalizedText ExperienceText { get; private set; }

        public override void Load()
        {
            ExcavationInterface = new();
            excavationUI = new();

            ClassInterface = new();
            classUI = new();

            mirrorUI = new();
            MagicMirrorInterface = new();
            MagicMirrorInterface.SetState(mirrorUI);

            string category = "UI";
            ExperienceText ??= Mod.GetLocalization($"{category}.ExperienceTracker");

        }

        public override void UpdateUI(GameTime gameTime)
        {
            ClassInterface?.Update(gameTime);
            MagicMirrorInterface?.Update(gameTime);

            if (!Main.gameMenu && Main.LocalPlayer != null && Main.LocalPlayer.active)
            {
                ExperiencePlayer expPlayer = Main.LocalPlayer.GetModPlayer<ExperiencePlayer>();
                SkillPlayer sPlayer = Main.LocalPlayer.GetModPlayer<SkillPlayer>();
                MissionPlayer mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
                mirrorUI.UpdatePlayers(expPlayer, sPlayer, mPlayer);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "ReverieMod: Magic Mirror UI",
                    delegate
                    {
                        MagicMirrorInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );

                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "ReverieMod: Class Selection UI",
                    delegate
                    {
                        ClassInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}