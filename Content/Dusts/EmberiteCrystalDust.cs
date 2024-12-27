using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Graphics.Shaders;

namespace Reverie.Content.Dusts
{
    public sealed class EmberiteCrystalDust : ModDust
    {
        public override string Texture => Assets.Dusts + Name;

        public override void OnSpawn(Dust dust)
        {
            dust.noLight = false;
            dust.noGravity = true;
            dust.noLightEmittence = false;
            dust.alpha = 175;
            dust.fadeIn = 0f;
            dust.scale = 0.11f;
            dust.velocity *= 0;
            dust.frame = new Rectangle(0, 0, 0, 0);
            dust.shader = new ArmorShaderData(new Ref<Effect>(Reverie.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
            dust.color = new Color(255, 255, 255);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
            => dust.color;

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.1f;

            dust.velocity.X += (float)Math.Sin(Main.time / 20) * 0.01f;
            dust.velocity.X = MathHelper.Clamp(dust.velocity.X, -0.5f, 0.5f);

            dust.velocity.Y -= 0.05f;
            dust.velocity.Y = Math.Max(dust.velocity.Y, -1f);
           
            if (dust.fadeIn > dust.scale)
            {
                dust.scale += 0.01f;
            }
            else
            {
                dust.fadeIn = 0;

                dust.scale -= 0.005f;
                if (dust.scale < 0.1f)
                {
                    dust.active = false;
                }
            }
            dust.shader.UseColor(dust.color * Utils.GetLerpValue(0, 4, dust.fadeIn, true));

            dust.fadeIn++;
            if (dust.fadeIn > 100)
                dust.active = false;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);

            return false;
        }
    }
}