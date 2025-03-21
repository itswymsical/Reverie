using Reverie.Common.Subworlds.Sylvanwalde;
using SubworldLibrary;


namespace Reverie.Content.Items.Debugging;

public class SylvanwaldeTranslocator : ModItem
{
    public override string Texture => "Terraria/Images/UI/Bestiary/Icon_Locked";
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool CanUseItem(Player player)
    {
        if (SubworldSystem.IsActive<SylvanSub>())
            return false;

        return base.CanUseItem(player);
    }
    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
            SubworldSystem.Enter<SylvanSub>();

        return true;
    }
}
