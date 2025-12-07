using ReLogic.Graphics;
using Reverie.Content.Items.Accessories;
using Reverie.Content.Tiles.Taiga;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;
using static System.Net.Mime.MediaTypeNames;
using Terraria.UI.Chat;

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
            HarvestNotificationManager.UpdateNotifications();

            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            foreach (var mission in missionPlayer.ActiveMissions())
            {
                mission.Update();
            }
        }
    }
    public override void AddRecipes()
    {
        Recipe iceBlade = Recipe.Create(ItemID.IceBlade);
        iceBlade.AddIngredient(ItemID.IceBlock, 30)
            .AddIngredient(ItemID.FallenStar, 4)
            .AddCondition(Condition.InSnow)
            .AddTile(TileID.IceMachine)
            .Register();

        Recipe fertilizer = Recipe.Create(ItemID.Fertilizer);
        fertilizer.AddIngredient(ItemID.PoopBlock, 3)
            .AddRecipeGroup(nameof(ItemID.Bass))
            .AddRecipeGroup(nameof(ItemID.DirtBlock), 3)
            .AddTile(TileID.WorkBenches)
            .Register();
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

    public void DrawReleaseInfo(SpriteBatch spriteBatch)
    {
        if (Main.gameMenu)
            return;

        Vector2 bottomAnchorPosition = new(Main.screenWidth / 2, Main.screenHeight - 20);
        DialogueManager.Instance.Draw(spriteBatch, bottomAnchorPosition);

        string dateString = DateTime.Now.ToString("MM.dd.yyyy");
        string text = $"Terraria: Reverie Alpha (v{dateString})";
        string subtitle = "— Content is subject to change —";

        var font = FontAssets.MouseText.Value;
        var scale = 0.85f;

        // Measure string and calculate centered position
        var textSize = font.MeasureString(text) * scale;
        Vector2 textPosition = new Vector2(
            Main.screenWidth / 2f - textSize.X / 2f,
            Main.screenHeight * 0.02f
        );

        var color = Color.Beige;
        var shadowColor = Color.Black;
        shadowColor.A = color.A;

        // Draw main text centered
        ChatManager.DrawColorCodedStringShadow(spriteBatch, font, text, textPosition, shadowColor, 0f, default, Vector2.One * scale);
        ChatManager.DrawColorCodedString(spriteBatch, font, text, textPosition, color, 0f, default, Vector2.One * scale);

        // Draw subtitle below main text, also centered
        var subtitleScale = 0.7f;
        var subtitleSize = font.MeasureString(subtitle) * subtitleScale;
        Vector2 subtitlePosition = new Vector2(
            Main.screenWidth / 2f - subtitleSize.X / 2f,
            textPosition.Y + textSize.Y + 4
        );

        ChatManager.DrawColorCodedStringShadow(spriteBatch, font, subtitle, subtitlePosition, shadowColor, 0f, default, Vector2.One * subtitleScale);
        ChatManager.DrawColorCodedString(spriteBatch, font, subtitle, subtitlePosition, color, 0f, default, Vector2.One * subtitleScale);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
        if (resourceBarIndex != -1)
        {
            layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                "Reverie: Pre-release info",
                delegate
                {
                    DrawReleaseInfo(Main.spriteBatch);
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}