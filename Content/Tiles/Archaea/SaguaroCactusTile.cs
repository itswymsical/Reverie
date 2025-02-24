using ReLogic.Content;

namespace Reverie.Content.Tiles.Archaea;

public class SaguaroCactusTile : ModCactus
{
    private Asset<Texture2D> texture;
    private Asset<Texture2D> fruitTexture;

    public override void SetStaticDefaults()
    {
        GrowsOnTileId = [ModContent.TileType<PrimordialSandTile>()];
        texture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/Tiles/Archaea/SaguaroCactusTile");
        fruitTexture = ModContent.Request<Texture2D>("Reverie/Content/Tiles/Plants/ExampleCactus_Fruit");
    }

    public override Asset<Texture2D> GetTexture() => texture;

    // This would be where the Cactus Fruit Texture would go, if we had one.
    public override Asset<Texture2D> GetFruitTexture() => null;
}
