using Reverie.Common.UI.Missions;
using Reverie.Core.Missions;
using Terraria.UI;

namespace Reverie.Content.Items.Debugging;

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

