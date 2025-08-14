using Reverie.Core.Graphics;
using Reverie.Core.Graphics.Interfaces;
using Reverie.Core.Loaders;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;

namespace Reverie.Content.Projectiles.Sharpnut;

public class SharpnutDaggerProj : ModProjectile, IDrawPrimitive
{
    public static int MaxStickies => 6;
    private bool thisDaggerChecked = false;
    protected NPC Target => Main.npc[(int)Projectile.ai[1]];

    protected bool stickToNPC;
    protected bool stickingToNPC;

    private Vector2 offset;
    private float oldRotation;

    private readonly float gravity = 0.125f;
    private bool arcCompleted = false;

    private bool fallOffStarted = false;
    private float spinRate = 0f;
    private readonly float fallOffGravity = 0.35f;

    private float prevVelocityY = 0f;
    private bool peakGamingUnlocked = false;

    private bool deathAnimationStarted = false;
    private int deathAnimationTimer = 0;
    private const int DEATH_ANIMATION_DURATION = 55;
    private Vector2 deathStartPosition;
    private Vector2 deathOffset;
    private float deathRotation;
    private float deathAlpha = 1f;

    private List<Vector2> cache;
    private Trail trail;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 3;
        ProjectileID.Sets.TrailingMode[Type] = 1;
    }

    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.hide = true;
        stickToNPC =
        Projectile.friendly =
        Projectile.tileCollide =
        Projectile.usesLocalNPCImmunity = true;

        Projectile.timeLeft = 240;
        Projectile.penetrate = Projectile.aiStyle = -1;
        Projectile.light = 0.2f;
        prevVelocityY = Projectile.velocity.Y;
    }

    public override void AI()
    {
        ManageCaches();
        ManageTrail();

        // Start death animation when time is almost up and stuck to target
        if (!deathAnimationStarted && stickingToNPC && Projectile.timeLeft <= 60)
        {
            deathAnimationStarted = true;
            deathAnimationTimer = 0;
            deathStartPosition = Projectile.Center;
            deathOffset = new Vector2(Main.rand.NextFloat(-20f, 20f), 0f);
            deathRotation = Projectile.rotation;
        }

        if (stickingToNPC && !deathAnimationStarted)
        {
            if (Target.active && !Target.dontTakeDamage)
            {
                Projectile.tileCollide = false;
                Projectile.Center = Target.Center - offset;
                Projectile.gfxOffY = Target.gfxOffY;
            }
            else
            {
                Projectile.Kill();
            }
        }
        else if (!stickingToNPC)
        {
            if (!arcCompleted)
            {
                // Check if 0.75 seconds (45 frames) have passed since spawn
                if (!fallOffStarted && Projectile.timeLeft <= 260 - 45)
                {
                    fallOffStarted = true;
                    spinRate = 0.1f;
                }

                if (fallOffStarted)
                {
                    Projectile.velocity.Y += fallOffGravity;

                    spinRate += 0.003f;
                    spinRate = Math.Min(spinRate, 0.3f);

                    float actualSpinRate = Projectile.velocity.X < 0 ? -spinRate : spinRate;
                    Projectile.rotation += actualSpinRate;

                    Projectile.velocity.X *= 0.985f;
                }
                else
                {
                    Projectile.velocity.Y += gravity;

                    Projectile.rotation = Projectile.velocity.ToRotation();

                    if (Projectile.velocity.X < 0)
                    {
                        Projectile.rotation += MathHelper.Pi;
                    }

                    Projectile.velocity.X *= 0.995f;
                }

                prevVelocityY = Projectile.velocity.Y;
            }
        }

        if (stickingToNPC && !deathAnimationStarted)
            Projectile.rotation = oldRotation;

        var player = Main.player[Projectile.owner];
        var distanceToPlayer = Vector2.Distance(Projectile.Center, player.Center);

        Projectile.spriteDirection = Projectile.direction;
    }

    public override bool? CanHitNPC(NPC target)
    {
        return !stickingToNPC && !target.friendly;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!stickingToNPC && stickToNPC)
        {
            if (!thisDaggerChecked)
            {
                float damageMultiplier = CalculateDamageMultiplier(target);
                modifiers.FinalDamage *= damageMultiplier;
                thisDaggerChecked = true;
            }

            Projectile.ai[1] = target.whoAmI;

            oldRotation = Projectile.velocity.ToRotation();

            offset = target.Center - Projectile.Center + Projectile.velocity * 0.75f;
            Projectile.velocity = Vector2.Zero;
            arcCompleted = true;
            stickingToNPC = true;
            Projectile.netUpdate = true;
        }
        else
        {
            RemoveStackProjectiles();
        }
    }

    private float CalculateDamageMultiplier(NPC target)
    {
        int stuckDaggers = 0;
        const float dmgBoost = 0.1f;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            var currentProjectile = Main.projectile[i];

            if (i != Projectile.whoAmI
                && currentProjectile.active
                && currentProjectile.type == Projectile.type
                && currentProjectile.ModProjectile is SharpnutDaggerProj otherDagger
                && otherDagger.stickingToNPC
                && otherDagger.Target.whoAmI == target.whoAmI)
            {
                stuckDaggers++;
            }
        }

        float multiplier = 1f + (stuckDaggers * dmgBoost);

        return Math.Min(multiplier, 2.5f);
    }

    protected void RemoveStackProjectiles()
    {
        var sticking = new Point[MaxStickies];
        var index = 0;

        for (var i = 0; i < Main.maxProjectiles; i++)
        {
            var currentProjectile = Main.projectile[i];

            if (i != Projectile.whoAmI
                && currentProjectile.active
                && currentProjectile.owner == Main.myPlayer
                && currentProjectile.type == Projectile.type
                && currentProjectile.ai[0] == 1f
                && currentProjectile.ai[1] == Target.whoAmI
            )
            {
                sticking[index++] = new Point(i, currentProjectile.timeLeft);

                if (index >= sticking.Length)
                    break;
            }
        }

        if (index >= sticking.Length)
        {
            var oldIndex = 0;

            for (var i = 1; i < sticking.Length; i++)
                if (sticking[i].Y < sticking[oldIndex].Y)
                    oldIndex = i;

            Main.projectile[sticking[oldIndex].X].Kill();
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (stickingToNPC)
        {
            Projectile.velocity = Vector2.Zero;
            arcCompleted = true;
            return false;
        }
        else
        {
            if (!deathAnimationStarted)
            {
                deathAnimationStarted = true;
                deathAnimationTimer = 0;
                deathStartPosition = Projectile.Center;
                deathOffset = new Vector2(Main.rand.NextFloat(-20f, 20f), 0f);
                deathRotation = Projectile.rotation;

                Projectile.velocity = Vector2.Zero;
                Projectile.friendly = false;
                Projectile.timeLeft = Math.Min(Projectile.timeLeft, 30);
            }

            return false;
        }
    }

    private void ManageCaches()
    {
        cache ??= Enumerable.Repeat(Projectile.Center, 15).ToList();

        Vector2 behindProjectile = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero);
        cache.Add(behindProjectile);

        if (cache.Count > 15)
            cache.RemoveAt(0);
    }

    private void ManageTrail()
    {
        // do not
        if (stickingToNPC)
            return;


        Color color = fallOffStarted ? Color.DimGray * 0.15f : Color.Black * 0.35f;

        trail ??= new Trail(Main.instance.GraphicsDevice, 15,
            new RoundedTip(15),
            factor => factor * 10,
            factor => color * 0.6f * (float)Math.Pow(factor.X, 2));

        trail.Positions = [.. cache];
        trail.NextPosition = Projectile.Center;
    }

    public void DrawPrimitives()
    {
        if (stickingToNPC)
            return;

        var effect = ShaderLoader.GetShader("LightningTrail").Value;

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
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;
        Vector2 drawPosition;
        float drawRotation;
        Color drawColor;

        var spriteEffects = SpriteEffects.None;
        var origin = new Vector2(22f, 9f);

        if (Projectile.spriteDirection == -1)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
            origin.X = texture.Width - origin.X;
        }

        if (deathAnimationStarted)
        {
            deathAnimationTimer++;

            float progress = (float)deathAnimationTimer / DEATH_ANIMATION_DURATION;

            Vector2 fallOffset = new Vector2(
                deathOffset.X * progress,
                deathOffset.Y + (progress * progress * 90f)
            );

            drawPosition = deathStartPosition + fallOffset - Main.screenPosition;
            drawRotation = deathRotation + (progress * MathHelper.TwoPi * 0.1f);

            deathAlpha = 1f - progress;
            drawColor = lightColor * deathAlpha;
        }
        else
        {
            drawPosition = Projectile.Center - Main.screenPosition;
            drawRotation = Projectile.rotation;
            drawColor = lightColor;
        }


        Main.EntitySpriteDraw(
            texture,
            drawPosition,
            null,
            drawColor,
            drawRotation,
            origin,
            Projectile.scale,
            spriteEffects,
            0
        );

        return false;
    }

    public override bool PreKill(int timeLeft)
    {
        return base.PreKill(timeLeft);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCs.Add(index);
    }
}