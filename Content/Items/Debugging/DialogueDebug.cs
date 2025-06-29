using Reverie.Core.Dialogue;
using System.Collections.Generic;
using Terraria.UI;

namespace Reverie.Content.Items.Debugging;

// Add this to your debug item to trace exactly where the freeze happens

public class FreezeTrackingDialogueDebug : ModItem
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
            DialogueManager.Instance.StartDialogue("AFallingStar.CrashLanding", 5, letterbox: true);
        }
        return true;
    }
}