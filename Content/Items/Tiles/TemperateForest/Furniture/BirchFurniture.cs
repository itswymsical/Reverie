using Reverie.Content.Tiles.TemperateForest.Furniture;

namespace Reverie.Content.Items.Tiles.TemperateForest.Furniture;

public class BirchWorkbenchItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchWorkbenchTile>());
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
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchTableTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(8)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class BirchSofaItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchSofaTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(5)
            .AddIngredient(ItemID.Silk, 2)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}

public class BirchBedItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchBedTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(15)
            .AddIngredient(ItemID.Silk, 5)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}

public class BirchSinkItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchSinkTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(6)
            .AddIngredient(ItemID.WaterBucket)
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
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchDoorClosedTile>());
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

public class BirchClockItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchClockTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<BirchWoodItem>(10)
            .AddIngredient(ItemID.Glass, 6)
            .AddRecipeGroup(RecipeGroupID.IronBar, 3)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}