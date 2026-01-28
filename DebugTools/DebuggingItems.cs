using Reverie.Common.UI.Missions;
using Reverie.Content.Cutscenes;
using Reverie.Core.Cinematics;
using Reverie.Core.Dialogue;
using Reverie.Core.Indicators;
using Reverie.Core.Missions;
using Terraria.UI;

namespace Reverie.DebugTools;

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
            var mission = mplayer.GetMission(MissionID.JourneysBegin);
            InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(mission));
        }
        return true;
    }
}