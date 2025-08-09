using Reverie.Core.Loaders;
using Terraria.Graphics.Effects;

namespace Reverie.Content.Tiles.OxiCopper;

public class CopperPlatingTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[TileID.CopperPlating][Type] = true;
        Main.tileMerge[Type][TileID.CopperPlating] = true;

        Main.tileMerge[TileID.CopperBrick][Type] = true;
        Main.tileMerge[Type][TileID.CopperBrick] = true;

        Main.tileBlockLight[Type] = true;

        DustType = DustID.CopperCoin;
        HitSound = SoundID.Tink;

        AddMapEntry(Color.Orange);
    }
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var effect = ShaderLoader.GetShader("ShineShader").Value;
        spriteBatch.End();
        spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect);


        if (effect != null)
        {
            effect.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.02f);
            effect.Parameters["uOpacity"]?.SetValue(1f);
        }
        
        return true;
    }
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(default, default, default, default, default, default);
    }
}
