using Terraria.GameContent;

namespace Reverie.Content.Menus;

public partial class IllustriousMenu
{
    private class EasterEggObject
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float Scale;
        public float Alpha;
        public int Type;

        public EasterEggObject(Vector2 position, Vector2 velocity, float scale)
        {
            Position = position;
            Velocity = velocity;
            Rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
            Scale = scale;
            Alpha = 0f;

            if (Main.rand.NextBool(1000000))
            {
                Type = 8;
            }
            else
            {
                Type = Main.rand.Next(8);
            }

            if (Type == 7)
            {
                Scale = 0.2f;
            }
        }

        public void Update()
        {
            Position += Velocity;

            Rotation += EASTER_EGG_ROTATION_SPEED;
            if (Rotation > MathHelper.TwoPi)
                Rotation -= MathHelper.TwoPi;

            if (Alpha < 1f)
                Alpha += 0.01f;
        }

        public Texture2D GetTexture()
        {
            return Type switch
            {
                0 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostMartian").Value,
                1 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostMeteorHead").Value,
                2 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}SpaceDolphin").Value,
                3 => TextureAssets.Item[ItemID.FirstFractal].Value,
                4 => TextureAssets.Item[ItemID.SDMG].Value,
                5 => TextureAssets.Item[ItemID.Bass].Value,
                6 => TextureAssets.Item[ItemID.AtlanticCod].Value,
                7 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostTree").Value,
                8 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}46").Value,

                _ => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostMartian").Value
            };
        }
    }
}