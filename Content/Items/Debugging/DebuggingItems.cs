using Reverie.Core.Indicators;
using Reverie.Core.Missions;

namespace Reverie.Content.Items.Debugging;

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

