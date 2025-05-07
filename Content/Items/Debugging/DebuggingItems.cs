﻿using Reverie.Common.UI.Missions;
using Reverie.Core.Entities;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Terraria.UI;
using Reverie.Content.Cutscenes;
using Reverie.Core.Cinematics;

namespace Reverie.Content.Items.Debugging;

public class DialogueTest : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
            DialogueManager.Instance.StartDialogue(
               NPCManager.MerchantData,
               DialogueKeys.Merchant.MerchantIntro,
               lineCount: 5,
               zoomIn: true, 
               modifications: 
               [(line: 1, delay: 3, emote: 0), 
                (line: 2, delay: 3, emote: 1),
                (line: 3, delay: 3, emote: 0),
                (line: 4, delay: 3, emote: 0),
                (line: 5, delay: 3, emote: 1)]);

        return true;
    }
}

public class PlayCutscene : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
            CutsceneSystem.PlayCutscene(new FallingStarCutscene());
        return true;
    }
}

public class MissionCompleteIndicator : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            var mplayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            var mission = mplayer.GetMission(MissionID.LightEmUp);
            InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(mission));
        }
        return true;
    }
}

public class SpawnUserInterfaceEntity : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        var mplayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        var mission = mplayer.GetMission(MissionID.LightEmUp);

        if (player.altFunctionUse != 0)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                MissionIndicatorManager.Instance.CreateIndicator(Main.MouseWorld, mission);
                Main.NewText($"Placed at position [X:{Main.MouseWorld.X} Y:{Main.MouseWorld.Y}]");
            }
           
        }
        else
        {
            Main.NewText($"[Cleared All Indicators] | Right-click to place an Indicator.");
            MissionIndicatorManager.Instance.ClearAllNotifications();
        }

        return true;
    }
    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
}
