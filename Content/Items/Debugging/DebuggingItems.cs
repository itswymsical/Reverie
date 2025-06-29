using Reverie.Common.UI.Missions;
using Reverie.Core.Indicators;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Terraria.UI;
using Reverie.Content.Cutscenes;
using Reverie.Core.Cinematics;
using Reverie.Core.Indicators;

namespace Reverie.Content.Items.Debugging;

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

public class SpawnMissionIndicator : ModItem
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
                ScreenIndicatorManager.Instance.CreateMissionIndicator(Main.MouseWorld, mission);
                Main.NewText($"Placed at position [X:{Main.MouseWorld.X} Y:{Main.MouseWorld.Y}]");
            }
           
        }
        else
        {
            Main.NewText($"[Cleared All Indicators] | Right-click to place an Indicator.");
            ScreenIndicatorManager.Instance.ClearAllIndicators();
        }

        return true;
    }
    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
}

public class SpawnDialogueIndicator : ModItem
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
        if (player.altFunctionUse != 0)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                ScreenIndicatorManager.Instance.CreateDialogueIndicator(
                 Main.MouseWorld,
                 NPCManager.GuideData,
                 DialogueKeys.FallingStar.Intro,
                 3,
                 zoomIn: true,
                 defaultDelay: 2,
                 defaultEmote: 0,
                 musicId: null
             );
                Main.NewText($"Placed at position [X:{Main.MouseWorld.X} Y:{Main.MouseWorld.Y}]");
            }

        }
        else
        {
            Main.NewText($"[Cleared All Indicators] | Right-click to place an Indicator.");
            ScreenIndicatorManager.Instance.ClearAllIndicators();
        }

        return true;
    }
    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
}
