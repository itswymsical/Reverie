using Reverie.Content.Items.Accessories;
using Reverie.Content.Tiles.Taiga;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Utilities;
using Terraria.GameContent;
using Terraria.Localization;

namespace Reverie.Common.Systems;

public class ReverieSystem : ModSystem
{
    public static ReverieSystem Instance => ModContent.GetInstance<ReverieSystem>();
    public static ModKeybind FFDialogueKeybind { get; private set; }
    public static ModKeybind SkipCutsceneKeybind { get; private set; }
    public override void Load()
    {
        FFDialogueKeybind = KeybindLoader.RegisterKeybind(Mod, "Fast-Forward Dialogue", "V");
        SkipCutsceneKeybind = KeybindLoader.RegisterKeybind(Mod, "Skip Cutscene", "Q");

    }
    public override void Unload()
    {
        FFDialogueKeybind = null;
        SkipCutsceneKeybind = null;
    }

    public override void PostUpdateWorld()
    {
        base.PostUpdateWorld();
        if (Main.LocalPlayer?.active == true && !Main.gameMenu)
        {
            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            foreach (var mission in missionPlayer.ActiveMissions())
            {
                mission.Update();
            }
        }
        HarvestNotificationManager.UpdateNotifications();
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

        RecipeGroup FishRecipeGroup = new(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.Bass)}", 2290,  2297, 2298, 2299, 2300, 2301, 2302, 2303, 2304, 2305, 2306, 2307, 
            2308, 2309, 2310, 2311, 2312, 2313, 2314, 2315, 2316, 2317, 2318, 2319, 2321, 4401, 4402);
        RecipeGroup.RegisterGroup(nameof(ItemID.Bass), FishRecipeGroup);

        RecipeGroup soilRecipeGroup = new(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.DirtBlock)}", 
            ItemID.DirtBlock, ItemID.MudBlock, ModContent.ItemType<PeatBlockItem>(), ItemID.AshBlock);
        RecipeGroup.RegisterGroup(nameof(ItemID.DirtBlock), soilRecipeGroup);
    }

    public override void PostDrawInterface(SpriteBatch spriteBatch)
    {
        if (Main.gameMenu)
            return;

        Vector2 bottomAnchorPosition = new(Main.screenWidth / 2, Main.screenHeight - 20);
        DialogueManager.Instance.Draw(spriteBatch, bottomAnchorPosition);

        string dateString = DateTime.Now.ToString("MM.dd.yyyy");
        string title = $"Reverie Demonstation (pre-release build v{dateString})";
        string subtitle = "(-- ALL CURRENT CONTENT IS SUBJECT TO CHANGE OR REMOVAL --)";

        var font = FontAssets.MouseText.Value;
        Vector2 titleSize = font.MeasureString(title) * 0.3f;
        Vector2 subtitleSize = font.MeasureString(subtitle) * 0.3f;

        Vector2 titlePos = new(Main.screenWidth / 2f - titleSize.X / 2f, Main.screenHeight / 24f - 8);
        Vector2 subtitlePos = new(Main.screenWidth / 1.93f - subtitleSize.X / 2f, Main.screenHeight / 24f + titleSize.Y + 8);

        DrawUtils.DrawText(spriteBatch, Color.Wheat, title, titlePos, 0.3f);
        DrawUtils.DrawText(spriteBatch, Color.Wheat, subtitle, subtitlePos, 0.3f);
    }
}