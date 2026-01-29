using Reverie.Common.Commands;
using Reverie.Common.UI.Missions;
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
            var mission = MissionCommandHelper.GetOrCreateMission(MissionID.JourneysBegin, player);

            if (mission != null)
            {
                InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(mission));
            }
            else
            {
                Main.NewText("Mission not found!", Color.Red);
            }
        }
        return true;
    }
}