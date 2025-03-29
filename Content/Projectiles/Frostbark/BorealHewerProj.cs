using Reverie.Core.Graphics;
using Reverie.Core.Interfaces;
using Reverie.Utilities;
using System.Collections.Generic;

using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;


namespace Reverie.Content.Projectiles.Frostbark;

public class BorealHewerProj : ModProjectile, IDrawPrimitive
{

    public override string Texture => $"Reverie/Assets/Textures/Items/Frostbark/BorealHewer";
    public int MaxStickies => 1;
    private bool isStuck = false;
    private bool isReturning = false;

    private int shakeTimer = 0;
    private const int SHAKE_DURATION = 30;
    private Vector2 originalPosition;
    private int stickTimer = 0;
    private int soundTimer = 0;
    protected NPC Target => Main.npc[(int)Projectile.ai[1]];

    protected bool stickToNPC;
    protected bool stickToTile;

    protected bool stickingToNPC;
    protected bool stickingToTile;

    private Vector2 offset;

    private float oldRotation;

    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;
    private Color color = new(255, 255, 255);
    private readonly Vector2 Size = new(46, 50);


    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 4;
        ProjectileID.Sets.TrailingMode[Type] = 4;
    }
    public override void SetDefaults()
    {
        Projectile.damage = 10;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.width = 42;
        Projectile.height = 32;

        stickToTile =
            stickToNPC =
            Projectile.friendly =
            Projectile.tileCollide =
            Projectile.usesLocalNPCImmunity = true;

        Projectile.localNPCHitCooldown = 20;
        Projectile.timeLeft = 660;
        Projectile.penetrate = Projectile.aiStyle = -1;
        Projectile.light = 0.2f;
        base.SetDefaults();
    }

    public override void AI()
    {
        ManageCaches();
        ManageTrail();
        if (stickingToNPC)
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

        if (stickingToTile || stickingToNPC)
            Projectile.rotation = oldRotation;

        var player = Main.player[Projectile.owner];
        var distanceToPlayer = Vector2.Distance(Projectile.Center, player.Center);
        soundTimer++;
        //DealTreeDamage(player);

        Projectile.spriteDirection = Projectile.direction;
        if (!isStuck && !isReturning)
        {

            if (soundTimer >= 14)
            {
                soundTimer = 0;
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.position);
            }

            Projectile.rotation += 0.4f * Projectile.direction;
            if (distanceToPlayer > 450f)
            {
                isReturning = true;
                HandleReturn(player);
            }
        }

        else if (isStuck && !isReturning)
        {
            stickTimer++;
            if (distanceToPlayer < 70f || stickTimer >= 20)
                InitiateReturn();
        }

        else if (isReturning) HandleReturn(player);
    }

    private void StickToTile()
    {
        isStuck = true;
        Projectile.velocity = Vector2.Zero;
        stickTimer = 0;
    }

    private void InitiateReturn()
    {
        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}HewerReturn")
        {
            MaxInstances = 1,
            Volume = 0.4f,
            Pitch = -0.9f,
        }, Projectile.position);

        Projectile.tileCollide = false;
        stickToNPC = false;
        stickToTile = false;
        stickingToNPC = false;
        isReturning = true;
        shakeTimer = SHAKE_DURATION;
        originalPosition = Projectile.position;
    }

    private void HandleReturn(Player player)
    {
        Projectile.tileCollide = false;
        stickToNPC = false;
        stickToTile = false;
        stickingToNPC = false;

        if (shakeTimer > 0)
        {
            Projectile.position = originalPosition + new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            shakeTimer--;
        }
        else
        {
            var directionToPlayer = player.Center - Projectile.Center;
            directionToPlayer.Normalize();
            var speed = 15f;
            Projectile.velocity = directionToPlayer * speed;

            Projectile.rotation += 0.4f;

            if (Projectile.Hitbox.Intersects(player.Hitbox))
            {
                Projectile.Kill();
            }
            if (soundTimer >= 14)
            {
                soundTimer = 0;
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.position);
            }
        }
    }

    private void DealTreeDamage(Player player)
    {
        var x = (int)(Projectile.Center.X / 16);
        var y = (int)(Projectile.Center.Y / 16);

        // Check if the tile is a tree
        if (Main.tile[x, y].TileType == TileID.Trees)
        {
            player.PickTile(x, y, 45);
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!stickingToNPC && !stickingToTile && stickToNPC)
        {
            Projectile.ai[1] = target.whoAmI;

            oldRotation = Projectile.rotation;

            offset = target.Center - Projectile.Center + Projectile.velocity * 0.75f;

            stickingToNPC = true;

            Projectile.netUpdate = true;
        }
        else
        {
            RemoveStackProjectiles();
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (!stickingToTile && !stickingToNPC && stickToTile)
        {
            oldRotation = Projectile.rotation;

            Projectile.velocity = Vector2.Zero;

            stickingToTile = true;
        }

        SoundEngine.PlaySound(SoundID.Item50, Projectile.position);
        StickToTile();
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(SoundID.Item50, target.position);
        if (hit.Crit)
        {
            target.AddBuff(BuffID.Frostburn, 40 + damageDone / 10);
        }
        isStuck = true;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.instance.LoadProjectile(Projectile.type);
        var texture = TextureAssets.Projectile[Projectile.type].Value;

        var drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (var k = 0; k < Projectile.oldPos.Length; k++)
        {
            var drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            var color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return false;
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

    private void ManageCaches()
    {
        var player = Main.LocalPlayer;
        var pos = Projectile.Center + player.DirectionTo(Projectile.Center) * (Size.Length() * Main.rand.NextFloat(0.5f, 1.1f)) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.0f, 4.0f);

        if (cache == null)
        {
            cache = [];

            for (var i = 0; i < 15; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);

        while (cache.Count > 15)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var player = Main.LocalPlayer;
        var pos = Projectile.Center + player.DirectionTo(Projectile.Center) * (Size.Length() * Main.rand.NextFloat(0.5f, 1.1f)) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.0f, 4.0f);

        trail ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(5), factor => factor * 16, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return new Color(color.R, color.G, color.B) * 0.4f * (float)Math.Pow(factor.X, 2) * (float)Math.Sin(Projectile.timeLeft / 150f * 4);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(5), factor => factor * 16, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return new Color(color.R, color.G, color.B) * 0.4f * (float)Math.Pow(factor.X, 2) * (float)Math.Sin(Projectile.timeLeft / 150f * 4);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos + Projectile.velocity;
        trail2.NextPosition = pos + Projectile.velocity;
    }

    public void DrawPrimitives()
    {
        var primitiveShader = Filters.Scene["LightningTrail"];
        if (primitiveShader != null)
        {
            var effect = primitiveShader.GetShader().Shader;
            if (effect != null)
            {
                var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
                var view = Main.GameViewMatrix.TransformationMatrix;
                var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.07f); //was originally 0.02, did not pop up as much. see if this change does anything
                effect.Parameters["repeats"]?.SetValue(8f);
                effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
                effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>("Reverie/Assets/Textures/VFX/EnergyTrail").Value);
                effect.Parameters["sampleTexture2"]?.SetValue(ModContent.Request<Texture2D>("Reverie/Assets/Textures/VFX/Bloom").Value);

                trail?.Render(effect);

                effect.Parameters["sampleTexture2"]?.SetValue(ModContent.Request<Texture2D>("Reverie/Assets/Textures/VFX/EnergyTrail").Value);

                trail2?.Render(effect);
            }
        }
    }
}