using Reverie.Utilities;

namespace Reverie.Content.Items.Frostbark;

[AutoloadEquip(EquipType.Head)]
public class FrostbarkHelmItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.width = 34;
        Item.height = 34;
        
        Item.defense = 3;
        
        Item.value = Item.sellPrice(copper: 35);
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return ItemUtils.IsArmorSet<FrostbarkHelmItem, FrostbarkHauberkItem, FrostbarkGreavesItem>(head, body, legs);
    }

    public override void UpdateArmorSet(Player player)
    {
        // TODO: Localize.
        player.setBonus = "Increases critical strike and melee speed by 6%";
        
        player.GetCritChance(DamageClass.Melee) += 6;
        player.GetAttackSpeed(DamageClass.Melee) += 0.06f;
    }
    
    public override void AddRecipes()
    {
        base.AddRecipes();
        
        CreateRecipe()
            .AddIngredient(ItemID.IceBlock, 12)
            .AddRecipeGroup(RecipeGroupID.IronBar, 6)
            .AddIngredient(ItemID.BorealWood, 8)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class FrostbarkHauberkItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.width = 34;
        Item.height = 34;
        
        Item.defense = 3;
        
        Item.value = Item.sellPrice(copper: 45);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.IceBlock, 8)
            .AddRecipeGroup(RecipeGroupID.IronBar, 5)
            .AddIngredient(ItemID.BorealWood, 16)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class FrostbarkGreavesItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.width = 34;
        Item.height = 34;
        
        Item.defense = 1;

        Item.value = Item.sellPrice(copper: 30);
    }
    
    public override void AddRecipes()
    {
        base.AddRecipes();
        
        CreateRecipe()
            .AddRecipeGroup(RecipeGroupID.IronBar, 4)
            .AddIngredient(ItemID.BorealWood, 16)
            .AddTile(TileID.Anvils)
            .Register();
    }
}