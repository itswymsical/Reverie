using Reverie.Content.Items.KingSlime;
using Terraria.GameContent.ItemDropRules;

namespace Reverie.Common.Items.Types;

public class GlobalLoot : GlobalItem
{
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (item.type == ItemID.KingSlimeBossBag)
        {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<GelatinousBlasterItem>(), 2));
        }
    }
}
