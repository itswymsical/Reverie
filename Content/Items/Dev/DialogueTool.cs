using Reverie.Core.Dialogue;
using Reverie.Core.Indicators;

namespace Reverie.Content.Items.Dev;

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
                ScreenIndicatorManager.Instance.CreateDialogueIndicator(Main.MouseWorld,
                    "Argie.Intro", 9);

                Main.NewText($"Placed at position [X:{Main.MouseWorld.X} Y:{Main.MouseWorld.Y}]");
            }
        }
        else
        {
            DialogueManager.Instance.StartDialogue("JourneysBegin.BasicsDone", 8, letterbox: false);

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
