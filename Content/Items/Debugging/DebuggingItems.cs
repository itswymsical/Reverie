﻿using Reverie.Common.UI.Missions;
using Reverie.Core.CustomEntities;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Terraria.UI;

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
            DialogueManager.Instance.StartDialogueByKey(
               NPCManager.MerchantData,
               DialogueKeys.FallingStar.MerchantIntro,
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
            var bb = mplayer.GetMission(MissionID.BloomcapHunt);
            InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(bb));
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
        var bb = mplayer.GetMission(MissionID.BloomcapHunt);

        if (player.altFunctionUse != 0)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                MissionIndicatorManager.Instance.CreateIndicator(Main.MouseWorld, bb);
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

public class ObjTest : ModItem
{
    public override string Texture => "Terraria/Images/UI/Bestiary/Icon_Locked";
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        var m = Main.LocalPlayer.GetModPlayer<MissionPlayer>().GetMission(MissionID.AFallingStar);
        if (Main.myPlayer == player.whoAmI)
            InGameNotificationsTracker.AddNotification(new MissionNotification(m));

        return true;
    }
}
