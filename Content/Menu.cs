using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Reverie.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content
{
    public class Menu : ModMenu
    {
        public override int Music => MusicLoader.GetMusicSlot(ReverieMusic.ReverieMusic.Instance, $"{Assets.Music}Resurgence");
        public override ModSurfaceBackgroundStyle MenuBackgroundStyle => null;
        public override Asset<Texture2D> SunTexture => null;
        public override Asset<Texture2D> MoonTexture => null;
        public override string DisplayName => "Reverie";

        public override void OnSelected() => SoundEngine.PlaySound(new SoundStyle($"{Assets.SFX.Directory}Theme_Select"));

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {

            var bg = TextureAssets.MagicPixel;
            spriteBatch.Draw(bg.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

            //Vector2 centerPosition = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            //Vector2 origin = new Vector2(ModContent.Request<Texture2D>($"{AssetPath}MenuTree").Width() / 2, Logo.Height() / 4);
            //spriteBatch.Draw(ModContent.Request<Texture2D>($"{AssetPath}MenuTree").Value, centerPosition, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);

            Main.dayTime = false;
            logoRotation = 0f;

            return true;
        }
    }
}