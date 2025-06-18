using Reverie.Content.Tiles.Rainforest.Surface.Trees;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Utilities;
using Terraria.DataStructures;
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
        NPCManager.Initialize();
        Reverie.Instance.Logger.Info("NPCManager for dialogue initialized...");

        FFDialogueKeybind = KeybindLoader.RegisterKeybind(Mod, "Fast-Forward Dialogue", "V");
        SkipCutsceneKeybind = KeybindLoader.RegisterKeybind(Mod, "Skip Cutscene", "Q");

    }
    public override void Unload()
    {
        FFDialogueKeybind = null;
        SkipCutsceneKeybind = null;
    }

    public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
    {
        base.ModifyTimeRate(ref timeRate, ref tileUpdateRate, ref eventUpdateRate);
        timeRate /= 2;
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