using Humanizer;
using Reverie.Core.Dialogue;
using Reverie.Utilities.Extensions;
using Terraria.Localization;

namespace Reverie.Common.Systems
{
    public class ReverieSystem : ModSystem
    {
        public override void Load()
        {
            On_Main.DrawInterface_36_Cursor += Main_DrawInterface_36_Cursor;
            NPCDataManager.Initialize();
            Reverie.Instance.Logger.Info("NPCDataManager initialized...");
        }

        public static ReverieSystem Instance => ModContent.GetInstance<ReverieSystem>();

        private void Main_DrawInterface_36_Cursor(On_Main.orig_DrawInterface_36_Cursor orig)
        {
            orig();

            if (Main.LocalPlayer.talkNPC >= 0)
            {
                NPC npc = Main.npc[Main.LocalPlayer.talkNPC];
                if (npc.active && npc.ModNPC == null)
                {
                    bool firstButtonClicked = false;
                    bool secondButtonClicked = false;

                    Rectangle shopButtonRectangle = new Rectangle(500, 560, 100, 30);
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        if (shopButtonRectangle.Contains(Main.mouseX, Main.mouseY))
                        {
                            firstButtonClicked = true;
                        }
                        Rectangle chatButtonRectangle = new Rectangle(610, 560, 100, 30);
                        if (chatButtonRectangle.Contains(Main.mouseX, Main.mouseY))
                        {
                            secondButtonClicked = true;
                        }
                    }

                    if (firstButtonClicked || secondButtonClicked)
                    {
                        npc.HandleWorldNPCChat(firstButtonClicked);
                    }
                }
            }
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
            Vector2 bottomAnchorPosition = new(Main.screenWidth / 2, Main.screenHeight - 20);
            DialogueManager.Instance.Draw(spriteBatch, bottomAnchorPosition);
        }
    }
}