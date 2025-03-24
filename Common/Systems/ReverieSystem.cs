using Reverie.Core.Dialogue;
using Terraria.Localization;

namespace Reverie.Common.Systems
{
    public class ReverieSystem : ModSystem
    {
        public override void Load()
        {
            NPCDataManager.Initialize();
            Reverie.Instance.Logger.Info("NPCDataManager initialized...");
        }
        public static ReverieSystem Instance => ModContent.GetInstance<ReverieSystem>();

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
            Vector2 bottomAnchorPosition = new(Main.screenWidth / 2, Main.screenHeight - 20);
            DialogueManager.Instance.Draw(spriteBatch, bottomAnchorPosition);
        }
    }
}