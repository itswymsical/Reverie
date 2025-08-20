using Reverie.Core.Tiles.Actors;

namespace Reverie.Core.Items.Components;

public abstract class FurnitureItem : ModItem
{
    protected abstract FurnitureType FurnitureType { get; }
    protected abstract int TileType { get; }
    protected abstract int MaterialType { get; }
    protected virtual int TorchType { get; }

    public override void SetDefaults()
    {
        Item.value = 150;
        Item.DefaultToPlaceableTile(TileType);
    }

    public override void AddRecipes()
    {
        var recipe = CreateRecipe();

        switch (FurnitureType)
        {
            case FurnitureType.Chair:
                recipe.AddIngredient(MaterialType, 4);
                recipe.AddTile(TileID.WorkBenches);
                break;

            case FurnitureType.Table:
                recipe.AddIngredient(MaterialType, 8);
                recipe.AddTile(TileID.WorkBenches);
                break;

            case FurnitureType.Workbench:
                recipe.AddIngredient(MaterialType, 10);
                break;

            case FurnitureType.Bed:
                recipe.AddIngredient(MaterialType, 15);
                recipe.AddIngredient(ItemID.Silk, 5);
                recipe.AddTile(TileID.Sawmill);
                break;

            case FurnitureType.Dresser:
                recipe.AddIngredient(MaterialType, 16);
                recipe.AddTile(TileID.Sawmill);
                break;

            case FurnitureType.Sink:
                recipe.AddIngredient(MaterialType, 6);
                recipe.AddIngredient(ItemID.WaterBucket);
                recipe.AddTile(TileID.WorkBenches);
                break;

            case FurnitureType.Bathtub:
                recipe.AddIngredient(MaterialType, 14);
                recipe.AddTile(TileID.Sawmill);
                break;

            case FurnitureType.Bookcase:
                recipe.AddIngredient(MaterialType, 20);
                recipe.AddIngredient(ItemID.Book, 10);
                recipe.AddTile(TileID.Sawmill);
                break;

            case FurnitureType.Piano:
                recipe.AddIngredient(ItemID.Bone, 4);
                recipe.AddIngredient(MaterialType, 15);
                recipe.AddIngredient(ItemID.Book);
                recipe.AddTile(TileID.Sawmill);
                break;

            case FurnitureType.Lamp:
                recipe.AddIngredient(MaterialType, 3);
                recipe.AddIngredient(ItemID.Torch);
                recipe.AddTile(TileID.WorkBenches);
                break;

            case FurnitureType.Sofa:
                recipe.AddIngredient(MaterialType, 5);
                recipe.AddIngredient(ItemID.Silk, 2);
                recipe.AddTile(TileID.Sawmill);
                break;

            case FurnitureType.Chandelier:
                recipe.AddIngredient(MaterialType, 4);
                recipe.AddIngredient(ItemID.Torch, 4);
                recipe.AddIngredient(ItemID.Chain);
                recipe.AddTile(TileID.Anvils);
                break;

            case FurnitureType.Candelabra:
                recipe.AddIngredient(MaterialType, 5);
                recipe.AddIngredient(ItemID.Torch, 5);
                recipe.AddTile(TileID.WorkBenches);
                break;

            case FurnitureType.Lantern:
                recipe.AddIngredient(MaterialType, 6);
                recipe.AddIngredient(ItemID.Torch, 1);
                recipe.AddTile(TileID.WorkBenches);
                break;

            case FurnitureType.Clock:
                recipe.AddIngredient(MaterialType, 10);
                recipe.AddIngredient(ItemID.Glass, 6);
                recipe.AddRecipeGroup(RecipeGroupID.IronBar, 3);
                recipe.AddTile(TileID.Sawmill);
                break;

            case FurnitureType.Chest:
                recipe.AddIngredient(MaterialType, 8);
                recipe.AddRecipeGroup(RecipeGroupID.IronBar, 2);
                recipe.AddTile(TileID.WorkBenches);
                break;

            case FurnitureType.Campfire:
                recipe.AddIngredient(MaterialType, 10);
                recipe.AddIngredient(TorchType, 5);
                break;

            case FurnitureType.Platform:
                recipe = CreateRecipe(2);
                recipe.AddIngredient(MaterialType);
                break;

            case FurnitureType.Torch:
                recipe = CreateRecipe(3);
                recipe.AddIngredient(MaterialType);
                recipe.AddIngredient(ItemID.Gel);
                break;

            case FurnitureType.ClosedDoor:
                recipe = CreateRecipe();
                recipe.AddIngredient(MaterialType, 6);
                recipe.AddTile(TileID.WorkBenches);
                break;
        }

        recipe.Register();
    }
}