using Reverie.Common.Items.Components;
using Reverie.Framework.Items.Components;

namespace Reverie.Content.Items.Shovels;

public class IronShovelItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.width = 32;
        Item.height = 32;

        Item.TryEnable(out ShovelItemComponent component);
    }
}