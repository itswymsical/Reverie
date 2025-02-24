using ReLogic.Content;

namespace Reverie.Content.Tiles.Archaea;

public class SaguaroCactusTile : ModCactus
{
    private Asset<Texture2D> texture;
    private Asset<Texture2D> fruitTexture;

    public override void SetStaticDefaults()
    {
        GrowsOnTileId = [ModContent.TileType<PrimordialSandTile>()];
        texture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/Tiles/Archaea/SaguaroCactus");
        fruitTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/Tiles/Archaea/SaguaroCactus_Fruit");
    }

    public override Asset<Texture2D> GetTexture() => texture;
    public override Asset<Texture2D> GetFruitTexture() => fruitTexture;
}
