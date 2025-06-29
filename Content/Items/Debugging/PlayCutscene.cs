using Reverie.Content.Cutscenes;
using Reverie.Core.Cinematics;

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

