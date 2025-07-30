using Reverie.Utilities.Extensions;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace Reverie.Content.Projectiles.Sharpnut;

public class AcornLauncherProj : ModProjectile
{
    private enum AIState
    {
        Charging,
        Firing
    }

    AIState State
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }

    private const float CHARGE_TIME = 34f;

    public override void SetDefaults()
    {
        Projectile.width = 92;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.ownerHitCheck = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.scale = 0.74f;
    }
    public override bool? CanDamage() => false;

    public override bool PreAI()
    {
        var owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || !owner.channel)
        {
            Projectile.Kill();
            return false;
        }

        var aimDirection = Vector2.Normalize(Main.MouseWorld - owner.Center);
        Projectile.Center = owner.Center;
        if (Main.myPlayer == Projectile.owner)
        {
            Projectile.direction = Main.MouseWorld.X > owner.Center.X ? 1 : -1;
            Projectile.netUpdate = true;
        }

        owner.ChangeDir(Projectile.direction);
        Projectile.spriteDirection = Projectile.direction;

        Projectile.rotation = aimDirection.ToRotation();

        if (Main.myPlayer == Projectile.owner)
        {
            if (State == AIState.Charging)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] >= CHARGE_TIME)
                {
                    State = AIState.Firing;
                }
            }
            else if (State == AIState.Firing)
            {
                if (owner.HasAmmo(owner.HeldItem))
                {
                    FireProjectile(owner, aimDirection);
                }
                State = AIState.Charging;
                Projectile.ai[1] = 0;
            }
        }

        SetOwnerAnimation(owner);

        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = ModContent.Request<Texture2D>(Texture).Value;
        var origin = new Vector2(texture.Width * 0.3f, texture.Height * 0.55f);
        var drawPos = Projectile.Center - Main.screenPosition;
        var spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

        Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

        return false;
    }
    private void FireProjectile(Player owner, Vector2 direction)
    {
        owner.ConsumeItem(ItemID.Acorn);
        var type = ModContent.ProjectileType<AcornProj>();
        var speed = owner.HeldItem.shootSpeed;
        var damage = owner.HeldItem.damage;
        var knockback = owner.HeldItem.knockBack;

        Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Center, direction * speed, type, damage, knockback, owner.whoAmI);

        // Play shoot sound
        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}AcornFire")
        {
            Volume = 1f,
            PitchVariance = 0.2f,
            MaxInstances = 2,
        }, owner.position);
    }

    private void SetOwnerAnimation(Player owner)
    {
        owner.ChangeDir(Projectile.direction);
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (owner.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2);
    }
}

public class AcornProj : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 18;
        Projectile.aiStyle = Projectile.extraUpdates = 1;
        Projectile.friendly = Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 600;
        AIType = ProjectileID.WoodenArrowFriendly;
    }
    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Main.instance.LoadProjectile(Projectile.type);
        var texture = TextureAssets.Projectile[Projectile.type].Value;

        Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (var k = 0; k < Projectile.oldPos.Length; k++)
        {
            var drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            var color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
    }

    public override void OnKill(int timeLeft)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        //DustExtensions.SpawnDustCloud(Projectile.position, Projectile.width, Projectile.height, DustID.t_LivingWood);
        for (var i = 0; i < 3; ++i)
        {
            Projectile.NewProjectile(default, Projectile.Center, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * 4f,
                ModContent.ProjectileType<AcornShrapnelProj>(), (int)(Projectile.damage * 0.5f), 0.5f, Projectile.owner);
        }
    }
}

public class AcornShrapnelProj : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
    }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 18;
        Projectile.aiStyle = 1;
        Projectile.friendly = Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 120;
        Projectile.penetrate = 1;
        Projectile.scale = 0.87f;
    }
    public override void AI()
    {
        Projectile.ai[1] += 1f;
        if (Projectile.ai[1] > 40f)
        {
            Projectile.Kill();
        }
        Projectile.velocity.Y = Projectile.velocity.Y + 0.2f;
        if (Projectile.velocity.Y > 18f)
        {
            Projectile.velocity.Y = 18f;
        }
        Projectile.velocity.X = Projectile.velocity.X * 0.98f;
        return;
    }


    public override void OnKill(int timeLeft)
    {
        //DustExtensions.SpawnDustCloud(Projectile.position, Projectile.width, Projectile.height, DustID.t_LivingWood);
    }
}
