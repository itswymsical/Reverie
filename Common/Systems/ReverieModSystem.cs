using Terraria;
using Terraria.ModLoader;
using Reverie.Core.Skills;
using Reverie.Core.Dialogue;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;

namespace Reverie.Common.Systems
{
    public class ReverieModSystem : ModSystem
    {
        public override void Load()
        {
            SkillList.Initialize();
            NPCDataManager.Initialize();
            Reverie.Instance.Logger.Info("NPCDataManager initialized...");
        }
        public static ReverieModSystem Instance => ModContent.GetInstance<ReverieModSystem>();

        public override void AddRecipeGroups()
        {
            RecipeGroup CopperBarRecipeGroup = new(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CopperBar)}",
                        ItemID.CopperBar, ItemID.TinBar);
            RecipeGroup.RegisterGroup(nameof(ItemID.CopperBar), CopperBarRecipeGroup);

            RecipeGroup SilverBarRecipeGroup = new(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}",
                        ItemID.SilverBar, ItemID.TungstenBar);
            RecipeGroup.RegisterGroup(nameof(ItemID.SilverBar), SilverBarRecipeGroup);
        }

        public override void PostUpdateWorld()
        {
            if (Main.netMode != NetmodeID.Server) // multiplayer support in the future
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