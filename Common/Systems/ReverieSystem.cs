using Reverie.Core.Dialogue;
using Reverie.Utilities;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace Reverie.Common.Systems
{
    public class ReverieSystem : ModSystem
    {
        public static ReverieSystem Instance => ModContent.GetInstance<ReverieSystem>();

        public override void Load()
        {
            NPCDataManager.Initialize();
            Reverie.Instance.Logger.Info("NPCDataManager for dialogue initialized...");
        }

        public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
        {
            base.ModifyTimeRate(ref timeRate, ref tileUpdateRate, ref eventUpdateRate);
            timeRate /= 2;
        }

        public override void AddRecipeGroups()
        {
            RecipeGroup CopperBarRecipeGroup = new(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CopperBar)}",
                        ItemID.CopperBar, ItemID.TinBar);
            RecipeGroup.RegisterGroup(nameof(ItemID.CopperBar), CopperBarRecipeGroup);

            RecipeGroup SilverBarRecipeGroup = new(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}",
                        ItemID.SilverBar, ItemID.TungstenBar);
            RecipeGroup.RegisterGroup(nameof(ItemID.SilverBar), SilverBarRecipeGroup);

            RecipeGroup GoldBarRecipeGroup = new(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.GoldBar)}",
                  ItemID.GoldBar, ItemID.PlatinumBar);
            RecipeGroup.RegisterGroup(nameof(ItemID.GoldBar), GoldBarRecipeGroup);
        }

        public override void PostUpdateWorld()
        {
            if (Main.netMode != NetmodeID.Server) // todo: multiplayer support.
            {
                DialogueManager.Instance.UpdateActive();
            }
            
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.gameMenu)
                return;

            Vector2 bottomAnchorPosition = new(Main.screenWidth / 2, Main.screenHeight - 20);
            DialogueManager.Instance.Draw(spriteBatch, bottomAnchorPosition);

            DrawUtils.DrawText(spriteBatch, Color.Wheat, "            - Reverie Demonstration (dev-alpha build v03.26.2025) -" +
                "\n- (All current content portrayed in Reverie is subject to change or removal) -", new(Main.screenWidth / 2, Main.screenHeight / 24f), 0.3f);
        }
    }
}