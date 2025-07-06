using Reverie.Content.Tiles.TemperateForest.Furniture;

namespace Reverie.Content.Items.Tiles.TemperateForest.Furniture;

public class BirchWorkbenchItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        //Item.DefaultToPlaceableTile(ModContent.TileType<BirchWorkbenchTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(10)
            .Register();
    }
}
public class BirchTableItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        //Item.DefaultToPlaceableTile(ModContent.TileType<BirchTableTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(8)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
public class BirchChairItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchChairTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(4)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
public class BirchDoorItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        //Item.DefaultToPlaceableTile(ModContent.TileType<BirchTableTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(6)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
public class BirchCandleItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        //Item.DefaultToPlaceableTile(ModContent.TileType<BirchTableTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(4)
            .AddIngredient(ItemID.Torch)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
public class BirchBookcaseItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        //Item.DefaultToPlaceableTile(ModContent.TileType<BirchTableTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(20)
            .AddIngredient(ItemID.Book, 10)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}