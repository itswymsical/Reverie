using Reverie.Common.Subworlds.Archaea;
using SubworldLibrary;


namespace Reverie.Content.Items;

public class BustedTranslocator : ModItem
{
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool CanUseItem(Player player)
    {
        if (SubworldSystem.IsActive<ArchaeaSub>())
            return false;

        return base.CanUseItem(player);
    }
    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
            SubworldSystem.Enter<ArchaeaSub>();

        return true;
    }
}
