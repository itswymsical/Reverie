using Reverie.Common.Players;

namespace Reverie.Content.Items.Lodestone;

[AutoloadEquip(EquipType.Legs)]
public class LodestoneLeggings : ModItem
{
    public override void SetDefaults()
    {
        Item.defense = 4;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 2);
        Item.width = Item.height = 34;
    }

    public override void UpdateEquip(Player player)
    {
        player.runAcceleration *= 1.1f;
    }

    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<LodestoneItem>(), 4);
        recipe.AddIngredient(ModContent.ItemType<MagnetizedCoil>(), 2);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class LodestoneChestplate : ModItem
{
    public override void SetDefaults()
    {
        Item.defense = 5;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 2);
        Item.width = Item.height = 34;
    }
    public override void UpdateEquip(Player player)
    {
        player.endurance += 0.02f;
    }
    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<LodestoneItem>(), 6);
        recipe.AddIngredient(ModContent.ItemType<MagnetizedCoil>(), 2);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class LodestoneHelmet : ModItem
{
    public override void SetStaticDefaults()
    {
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
    }
    public override void SetDefaults()
    {
        Item.defense = 3;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 2);
        Item.width = Item.height = 34;
    }

    public override void UpdateEquip(Player player)
    {
        base.UpdateEquip(player);
        player.pickSpeed += 0.1f;
    }
    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return body.type == ModContent.ItemType<LodestoneChestplate>() && legs.type == ModContent.ItemType<LodestoneLeggings>();
    }

    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = "You emit a magnetic field that gravitates items towards you" +
            "\nIncreases pickup range substantially";
        for (var i = 0; i < Main.maxItems; i++)
        {
            var targetItem = Main.item[i];

            if (targetItem.active && !targetItem.beingGrabbed && targetItem.noGrabDelay == 0)
            {
                var distance = Vector2.Distance(player.Center, targetItem.Center);
                if (distance <= 120f)
                {
                    var movement = Vector2.Normalize(player.Center - targetItem.Center);
                    var speedFactor = 1f - distance / 120f;
                    var speed = 3f * speedFactor;
                    targetItem.velocity = movement * speed;
                }
            }
        }
    }
    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<LodestoneItem>(), 3);
        recipe.AddIngredient(ModContent.ItemType<MagnetizedCoil>(), 3);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}