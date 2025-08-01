namespace Reverie.Content.Items.Accessories;

[AutoloadEquip(EquipType.Neck)]
public class TopazPendant : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.accessory = true;
        Item.width = Item.height = 30;
        Item.rare = ItemRarityID.White;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var reqMana = 60;
        var currentMana = (float)player.statMana / player.statManaMax2;

        if (player.statManaMax2 >= reqMana)
        {
            var moveSpeed = 0.02f + (currentMana * 0.12f);
            player.moveSpeed += moveSpeed;

            var runAcceleration = 0.02f + (currentMana * 0.12f);
            player.runAcceleration += runAcceleration;
        }

        for (var i = 0; i < Main.maxItems; i++)
        {
            var targetItem = Main.item[i];

            if (targetItem.active && !targetItem.beingGrabbed && targetItem.noGrabDelay == 0)
            {
                var distance = Vector2.Distance(player.Center, targetItem.Center);
                if (distance <= 60f)
                {
                    var movement = Vector2.Normalize(player.Center - targetItem.Center);
                    var speed = 3f;
                    targetItem.velocity = movement * speed;
                }
            }
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Topaz, 2)
            .AddIngredient(ItemID.TinBar, 5)
            .AddIngredient(ItemID.Chain, 2)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
