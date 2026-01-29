using Reverie.Core.Graphics;
using Reverie.Core.Graphics.Interfaces;
using Reverie.Core.Loaders;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.Projectiles.Botany;

public class DaybloomSeedProj : ModProjectile, IDrawPrimitive
{
    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 8;

        Projectile.aiStyle = 1;
        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Ranged;

        Projectile.timeLeft = 600;

        AIType = ProjectileID.Seed;
    }

    public override void AI()
    {
        base.AI();

        ManageCaches();
        ManageTrail();
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var owner = Main.player[Projectile.owner];

        if (hit.Crit)
        {
            owner.AddBuff(BuffID.Ironskin, 5 * 60);
        }
    }
    private void ManageCaches()
    {
        var pos = Projectile.Center;

        if (cache == null)
        {
            cache = [];
            for (var i = 0; i < 11; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);
        while (cache.Count > 11)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var pos = Projectile.Center;

        var trailColor = new Color(244, 212, 0);

        trail ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 20, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return trailColor * 0.6f * (float)Math.Pow(factor.X, 2);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 8, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return Color.Lerp(trailColor, Color.White, 0.6f) * 0.7f * (float)Math.Pow(factor.X, 2);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos;
        trail2.NextPosition = pos;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.07f);
            effect.Parameters["repeats"]?.SetValue(8f);
            effect.Parameters["pixelation"]?.SetValue(4f);
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["pixelation"]?.SetValue(6f);
            trail2?.Render(effect);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;

        var drawPosition = Projectile.Center - offset - Main.screenPosition;
        var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
        var origin = sourceRect.Size() / 2f;

        Main.EntitySpriteDraw(
            texture,
            drawPosition,
            sourceRect,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
    }
}

public class BlinkrootSeedProj : ModProjectile, IDrawPrimitive
{
    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 8;

        Projectile.aiStyle = 1;
        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Ranged;

        Projectile.timeLeft = 600;

        AIType = ProjectileID.Seed;
    }

    public override void AI()
    {
        base.AI();

        ManageCaches();
        ManageTrail();
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var owner = Main.player[Projectile.owner];

        if (hit.Crit)
        {
            owner.AddBuff(BuffID.Swiftness, 4 * 60);
            owner.AddBuff(BuffID.Hunter, 5 * 60);
        }
    }
    private void ManageCaches()
    {
        var pos = Projectile.Center;

        if (cache == null)
        {
            cache = [];
            for (var i = 0; i < 11; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);
        while (cache.Count > 11)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var pos = Projectile.Center;

        var trailColor = new Color(212, 151, 32);

        trail ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 20, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return trailColor * 0.6f * (float)Math.Pow(factor.X, 2);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 8, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return Color.Lerp(trailColor, Color.White, 0.6f) * 0.7f * (float)Math.Pow(factor.X, 2);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos;
        trail2.NextPosition = pos;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"]?.SetValue(8f);
            effect.Parameters["pixelation"]?.SetValue(4f);
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["pixelation"]?.SetValue(6f);
            trail2?.Render(effect);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;

        var drawPosition = Projectile.Center - offset - Main.screenPosition;
        var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
        var origin = sourceRect.Size() / 2f;

        Main.EntitySpriteDraw(
            texture,
            drawPosition,
            sourceRect,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
    }
}

public class FireblossomSeedProj : ModProjectile, IDrawPrimitive
{
    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 8;

        Projectile.aiStyle = 1;
        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Ranged;

        Projectile.timeLeft = 600;

        AIType = ProjectileID.Seed;
    }

    public override void AI()
    {
        base.AI();

        ManageCaches();
        ManageTrail();
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var owner = Main.player[Projectile.owner];

        if (hit.Crit)
        {
            target.AddBuff(BuffID.OnFire, 4 * 60);
        }
    }

    private void ManageCaches()
    {
        var pos = Projectile.Center;

        if (cache == null)
        {
            cache = [];
            for (var i = 0; i < 13; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);
        while (cache.Count > 13)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var pos = Projectile.Center;

        var trailColor = new Color(215, 68, 29);

        trail ??= new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(16), factor => factor * 24, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return trailColor * 0.6f * (float)Math.Pow(factor.X, 2);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(16), factor => factor * 18, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return Color.Lerp(trailColor, Color.White, 0.6f) * 0.7f * (float)Math.Pow(factor.X, 2);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos;
        trail2.NextPosition = pos;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.07f);
            effect.Parameters["repeats"]?.SetValue(8f);
            effect.Parameters["pixelation"]?.SetValue(4f);
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}FireTrail").Value);

            trail?.Render(effect);

            effect.Parameters["pixelation"]?.SetValue(6f);
            trail2?.Render(effect);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;

        var drawPosition = Projectile.Center - offset - Main.screenPosition;
        var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
        var origin = sourceRect.Size() / 2f;

        Main.EntitySpriteDraw(
            texture,
            drawPosition,
            sourceRect,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
    }
}

public class ShiverthornSeedProj : ModProjectile, IDrawPrimitive
{
    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 8;

        Projectile.aiStyle = 1;
        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Ranged;

        Projectile.timeLeft = 600;

        AIType = ProjectileID.Seed;
    }

    public override void AI()
    {
        base.AI();

        ManageCaches();
        ManageTrail();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var owner = Main.player[Projectile.owner];

        if (hit.Crit)
        {
            target.AddBuff(BuffID.Frostburn, 4 * 60);
        }
    }

    private void ManageCaches()
    {
        var pos = Projectile.Center;

        if (cache == null)
        {
            cache = [];
            for (var i = 0; i < 11; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);
        while (cache.Count > 11)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var pos = Projectile.Center;

        var trailColor = new Color(92, 183, 255);

        trail ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 17, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return trailColor * 0.6f * (float)Math.Pow(factor.X, 2);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 8, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return Color.Lerp(trailColor, Color.White, 0.6f) * 0.7f * (float)Math.Pow(factor.X, 2);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos;
        trail2.NextPosition = pos;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.07f);
            effect.Parameters["repeats"]?.SetValue(8f);
            effect.Parameters["pixelation"]?.SetValue(4f);
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["pixelation"]?.SetValue(6f);
            trail2?.Render(effect);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;

        var drawPosition = Projectile.Center - offset - Main.screenPosition;
        var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
        var origin = sourceRect.Size() / 2f;

        Main.EntitySpriteDraw(
            texture,
            drawPosition,
            sourceRect,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
    }
}

public class MoonglowSeedProj : ModProjectile, IDrawPrimitive
{
    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 8;

        Projectile.aiStyle = 1;
        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Ranged;

        Projectile.timeLeft = 600;

        AIType = ProjectileID.Seed;
    }

    public override void AI()
    {
        base.AI();

        ManageCaches();
        ManageTrail();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var owner = Main.player[Projectile.owner];

        if (hit.Crit)
        {
            owner.AddBuff(BuffID.Regeneration, 8 * 60);
        }
    }
    private void ManageCaches()
    {
        var pos = Projectile.Center;

        if (cache == null)
        {
            cache = [];
            for (var i = 0; i < 11; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);
        while (cache.Count > 11)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var pos = Projectile.Center;

        var trailColor = new Color(52, 196, 187);

        trail ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 20, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return trailColor * 0.6f * (float)Math.Pow(factor.X, 2);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 8, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return Color.Lerp(trailColor, Color.White, 0.6f) * 0.7f * (float)Math.Pow(factor.X, 2);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos;
        trail2.NextPosition = pos;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.07f);
            effect.Parameters["repeats"]?.SetValue(8f);
            effect.Parameters["pixelation"]?.SetValue(4f);
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}BeamTrail").Value);

            trail?.Render(effect);

            effect.Parameters["pixelation"]?.SetValue(6f);
            trail2?.Render(effect);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;

        var drawPosition = Projectile.Center - offset - Main.screenPosition;
        var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
        var origin = sourceRect.Size() / 2f;

        Main.EntitySpriteDraw(
            texture,
            drawPosition,
            sourceRect,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
    }
}

public class DeathweedSeedProj : ModProjectile, IDrawPrimitive
{
    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 8;

        Projectile.aiStyle = 1;
        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Ranged;

        Projectile.timeLeft = 600;

        AIType = ProjectileID.Seed;
    }

    public override void AI()
    {
        base.AI();

        ManageCaches();
        ManageTrail();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var owner = Main.player[Projectile.owner];

        if (hit.Crit)
        {
            target.AddBuff(BuffID.Poisoned, 5 * 60);
        }
    }
    private void ManageCaches()
    {
        var pos = Projectile.Center;

        if (cache == null)
        {
            cache = [];
            for (var i = 0; i < 11; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);
        while (cache.Count > 11)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var pos = Projectile.Center;

        var trailColor = new Color(177, 92, 182);

        trail ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 20, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return trailColor * 0.6f * (float)Math.Pow(factor.X, 2);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 8, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return Color.Lerp(trailColor, Color.White, 0.6f) * 0.7f * (float)Math.Pow(factor.X, 2);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos;
        trail2.NextPosition = pos;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * .07f);
            effect.Parameters["repeats"]?.SetValue(8f);
            effect.Parameters["pixelation"]?.SetValue(4f);
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}BeamTrail").Value);

            trail?.Render(effect);

            effect.Parameters["pixelation"]?.SetValue(6f);
            trail2?.Render(effect);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;

        var drawPosition = Projectile.Center - offset - Main.screenPosition;
        var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
        var origin = sourceRect.Size() / 2f;

        Main.EntitySpriteDraw(
            texture,
            drawPosition,
            sourceRect,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
    }
}

public class WaterleafSeedProj : ModProjectile, IDrawPrimitive
{
    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 8;

        Projectile.aiStyle = 1;
        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Ranged;

        Projectile.timeLeft = 600;

        AIType = ProjectileID.Seed;
    }

    public override void AI()
    {
        base.AI();

        ManageCaches();
        ManageTrail();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var owner = Main.player[Projectile.owner];

        if (hit.Crit)
        {
            target.AddBuff(BuffID.Wet, 5 * 60);
        }
    }
    private void ManageCaches()
    {
        var pos = Projectile.Center;

        if (cache == null)
        {
            cache = [];
            for (var i = 0; i < 11; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);

        while (cache.Count > 11)
        {
            cache.RemoveAt(0);
        }
    }
    private void ManageTrail()
    {
        var pos = Projectile.Center;

        var trailColor = new Color(100, 150, 8);

        trail ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 12, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return trailColor * 0.6f * (float)Math.Pow(factor.X, 2);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 11, new RoundedTip(5), factor => factor * 5, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return Color.Lerp(trailColor, Color.White, 0.6f) * 0.7f * (float)Math.Pow(factor.X, 2);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos;
        trail2.NextPosition = pos;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.07f);
            effect.Parameters["repeats"]?.SetValue(8f);
            effect.Parameters["pixelation"]?.SetValue(4f);
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}WaterTrail").Value);

            trail?.Render(effect);

            effect.Parameters["pixelation"]?.SetValue(6f);
            trail2?.Render(effect);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;

        var drawPosition = Projectile.Center - offset - Main.screenPosition;
        var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
        var origin = sourceRect.Size() / 2f;

        Main.EntitySpriteDraw(
            texture,
            drawPosition,
            sourceRect,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
    }
}