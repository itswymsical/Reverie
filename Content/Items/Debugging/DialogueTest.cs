using Reverie.Common.UI.Missions;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Terraria.UI;

namespace Reverie.Content.Items.Debugging;

public class DialogueTest : ModItem
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
        if (Main.myPlayer == player.whoAmI)
            DialogueManager.Instance.StartDialogueByKey(
               NPCDataManager.GuideData,
               DialogueKeys.CrashLanding.SettlingIn,
               lineCount: 5,
               zoomIn: true
           );

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
        var m = Main.LocalPlayer.GetModPlayer<MissionPlayer>().GetMission(MissionID.A_FALLING_STAR);
        if (Main.myPlayer == player.whoAmI)
            InGameNotificationsTracker.AddNotification(new MissionNotification(m));

        return true;
    }
}
