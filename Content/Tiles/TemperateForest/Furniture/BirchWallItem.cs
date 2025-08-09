using Reverie.Content.Dusts;

namespace Reverie.Content.Tiles.TemperateForest.Furniture;

public class BirchWallItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Item.sellPrice(0);
        Item.DefaultToPlaceableWall(ModContent.WallType<BirchWoodWall>());
    }
    public override void AddRecipes()
    {
        CreateRecipe(4)
            .AddIngredient<BirchWoodItem>()
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class BirchFenceItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Item.sellPrice(0);
        Item.DefaultToPlaceableWall(ModContent.WallType<BirchFenceWall>());
    }
    public override void AddRecipes()
    {
        CreateRecipe(4)
            .AddIngredient<BirchWoodItem>()
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class BirchFenceWall : ModWall
{
    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        DustType = ModContent.DustType<BirchDust>();

        AddMapEntry(new Color(107, 89, 84));
        RegisterItemDrop(ModContent.ItemType<BirchFenceItem>());

    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}