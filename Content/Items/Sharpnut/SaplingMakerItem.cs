using Reverie.Content.Projectiles.Sharpnut;
using Reverie.Core.Cinematics.Camera;
using Reverie.Core.Graphics;
using Reverie.Core.Graphics.Interfaces;
using Reverie.Core.Loaders;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.Items.Sharpnut;

public class SaplingMakerItem : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 7;
        Item.width = Item.height = 38;
        Item.useTime = Item.useAnimation = 38;
        Item.knockBack = 7.8f;
        Item.crit = -2;
        Item.value = Item.sellPrice(silver: 14);
        Item.rare = ItemRarityID.Blue;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.DD2_MonkStaffSwing;
        Item.shootSpeed = 10.5f;

        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.noUseGraphic =
            Item.channel = Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<SaplingMakerProj>();
    }

    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.Acorn, 8);
        recipe.AddIngredient(ItemID.Wood, 20);
        recipe.AddTile(TileID.LivingLoom);
        recipe.Register();
    }
}

public class SaplingMakerProj : ModProjectile, IDrawPrimitive
{
    #region Configuration & Constants

    private const string CHAIN_TEXTURE_PATH = "Reverie/Assets/Textures/Items/Sharpnut/SaplingMakerProj_Chain";

    private const float CHARGE_INTERVAL = 60f;
    private const float MAX_CHARGE_TIME = 90f;
    private const int MAX_CHARGE_LEVEL = 3;
    private const float MAX_CHARGE_HOLD_TIME = 180f; // 3 seconds to force launch

    private const int LAUNCH_TIME_LIMIT = 18;
    private const float MAX_LAUNCH_LENGTH = 800f;
    private const float BASE_LAUNCH_SPEED = 6f;
    private const float MAX_LAUNCH_SPEED = 15f;
    private const float RETRACT_ACCEL = 3f;
    private const float MAX_RETRACT_SPEED = 20f;
    private const float FORCED_RETRACT_ACCEL = 6f;
    private const float MAX_FORCED_RETRACT_SPEED = 25f;
    private const float GRAVITY = 0.98f;
    private const float AIR_FRICTION = 0.97f;

    private const int SPIN_HIT_COOLDOWN = 20;
    private const int MOVING_HIT_COOLDOWN = 10;
    private const int DEFAULT_HIT_COOLDOWN = 30;
    private const float MAX_COLLISION_COUNT = 10f;
    private const float BASE_SPIN_RADIUS = 30f;
    private const float BASE_HIT_RADIUS = 55f;

    private const int TRAIL_LENGTH = 25;
    private const int TRAIL_TIP_SIZE = 16;

    #endregion

    #region Properties & Fields

    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;

    private enum AIState { Spinning, LaunchingForward, Retracting, ForcedRetracting }

    private AIState CurrentAIState
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }

    public ref float StateTimer => ref Projectile.ai[1];
    public ref float CollisionCounter => ref Projectile.localAI[0];
    public ref float SpinningStateTimer => ref Projectile.localAI[1];

    // Computed properties using better math
    private int ChargeLevel => Math.Min(MAX_CHARGE_LEVEL, (int)(SpinningStateTimer / CHARGE_INTERVAL));
    private float ChargeProgress => Math.Min(1f, SpinningStateTimer / MAX_CHARGE_TIME);
    private float ChargeMultiplier => 1f + ChargeLevel * 0.5f;
    private float LaunchSpeed => MathHelper.Lerp(BASE_LAUNCH_SPEED, MAX_LAUNCH_SPEED, ChargeProgress);
    private float SpinRadius => BASE_SPIN_RADIUS + ChargeLevel * 5f;
    private float HitRadius => BASE_HIT_RADIUS + ChargeLevel * 10f;

    #endregion

    #region Setup & Initialization

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.netImportant = true;
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = DEFAULT_HIT_COOLDOWN;
    }

    #endregion

    #region Core AI Logic

    public override void AI()
    {
        var player = Main.player[Projectile.owner];

        if (ShouldKillProjectile(player))
        {
            Projectile.Kill();
            return;
        }

        UpdateVisualEffects();

        var mountedCenter = player.MountedCenter;
        var meleeSpeed = player.GetTotalAttackSpeed(DamageClass.Melee);

        ExecuteCurrentState(player, mountedCenter, meleeSpeed);
        UpdateProjectileTransform(mountedCenter);
        UpdatePlayerAnimation(player, mountedCenter);
    }

    private bool ShouldKillProjectile(Player player) =>
        !player.active || player.dead || player.noItems || player.CCed ||
        Projectile.DistanceSQ(player.Center) > 900f * 900f ||
        (Main.myPlayer == Projectile.owner && Main.mapFullscreen);

    private void ExecuteCurrentState(Player player, Vector2 mountedCenter, float meleeSpeed)
    {
        switch (CurrentAIState)
        {
            case AIState.Spinning:
                HandleSpinning(player, mountedCenter);
                break;
            case AIState.LaunchingForward:
                HandleLaunching(player, mountedCenter);
                break;
            case AIState.Retracting:
                HandleRetracting(player, mountedCenter, meleeSpeed);
                break;
            case AIState.ForcedRetracting:
                HandleForcedRetracting(player, mountedCenter, meleeSpeed);
                break;
        }
    }

    #endregion

    #region State Handlers

    private void HandleSpinning(Player player, Vector2 mountedCenter)
    {
        if (Projectile.owner == Main.myPlayer)
        {
            var mouseDir = Vector2.Normalize(Main.MouseWorld - mountedCenter);
            if (mouseDir == Vector2.Zero) mouseDir = Vector2.UnitX * player.direction;

            player.ChangeDir(Math.Sign(mouseDir.X));

            // Force launch after max charge hold time or if player stops channeling
            bool shouldForceLaunch = SpinningStateTimer >= MAX_CHARGE_HOLD_TIME && ChargeLevel >= MAX_CHARGE_LEVEL;

            if (!player.channel || shouldForceLaunch)
            {
                LaunchProjectile(mouseDir, mountedCenter, player);
                return;
            }
        }

        UpdateSpinning(player, mountedCenter);
    }

    private void LaunchProjectile(Vector2 direction, Vector2 mountedCenter, Player player)
    {
        CurrentAIState = AIState.LaunchingForward;
        StateTimer = 0f;
        Projectile.velocity = direction * LaunchSpeed + player.velocity;
        Projectile.Center = mountedCenter;
        Projectile.netUpdate = true;
        Projectile.ResetLocalNPCHitImmunity();
        Projectile.localNPCHitCooldown = MOVING_HIT_COOLDOWN;
    }

    private void UpdateSpinning(Player player, Vector2 mountedCenter)
    {
        SpinningStateTimer++;

        // Check for charge level sound
        if (ChargeLevel > (int)((SpinningStateTimer - 1f) / CHARGE_INTERVAL))
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.position);

        // Calculate spinning position using polar coordinates
        float spinSpeed = MathHelper.Lerp(2f, 5f, ChargeLevel / (float)MAX_CHARGE_LEVEL);
        float angle = MathHelper.TwoPi * spinSpeed * SpinningStateTimer / 60f * player.direction;
        var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle) * 0.8f) * player.direction;

        // Flatten bottom arc for better feel
        if (offset.Y * player.gravDir > 0f)
            offset.Y *= 0.5f;

        Projectile.Center = mountedCenter + offset * SpinRadius + new Vector2(0, player.gfxOffY);
        Projectile.velocity = Vector2.Zero;
        Projectile.localNPCHitCooldown = SPIN_HIT_COOLDOWN;
    }

    private void HandleLaunching(Player player, Vector2 mountedCenter)
    {
        StateTimer++;
        bool shouldRetract = StateTimer >= LAUNCH_TIME_LIMIT ||
                           Projectile.DistanceSQ(mountedCenter) >= MAX_LAUNCH_LENGTH * MAX_LAUNCH_LENGTH;

        if (shouldRetract)
            TransitionTo(AIState.Retracting, 0.3f);

        player.ChangeDir(Math.Sign(Projectile.Center.X - player.Center.X));
        Projectile.localNPCHitCooldown = MOVING_HIT_COOLDOWN;
    }

    private void HandleRetracting(Player player, Vector2 mountedCenter, float meleeSpeed)
    {
        var toPlayer = mountedCenter - Projectile.Center;
        float distance = toPlayer.Length();

        if (distance <= MAX_RETRACT_SPEED * meleeSpeed)
        {
            Projectile.Kill();
            return;
        }

        var targetVel = Vector2.Normalize(toPlayer) * MAX_RETRACT_SPEED * meleeSpeed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity * 0.98f, targetVel, RETRACT_ACCEL * meleeSpeed / 60f);

        player.ChangeDir(Math.Sign(Projectile.Center.X - player.Center.X));
    }

    private void HandleForcedRetracting(Player player, Vector2 mountedCenter, float meleeSpeed)
    {
        Projectile.tileCollide = false;
        var toPlayer = mountedCenter - Projectile.Center;
        float distance = toPlayer.Length();

        if (distance <= MAX_FORCED_RETRACT_SPEED * meleeSpeed)
        {
            Projectile.Kill();
            return;
        }

        var targetVel = Vector2.Normalize(toPlayer) * MAX_FORCED_RETRACT_SPEED * meleeSpeed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity * 0.98f, targetVel, FORCED_RETRACT_ACCEL * meleeSpeed / 60f);

        // Check overshoot using dot product
        var futurePos = Projectile.Center + Projectile.velocity;
        if (Vector2.Dot(Vector2.Normalize(toPlayer), Vector2.Normalize(mountedCenter - futurePos)) < 0f)
        {
            Projectile.Kill();
            return;
        }

        player.ChangeDir(Math.Sign(Projectile.Center.X - player.Center.X));
    }

    private void TransitionTo(AIState newState, float velocityMultiplier)
    {
        CurrentAIState = newState;
        StateTimer = 0f;
        Projectile.netUpdate = true;
        Projectile.velocity *= velocityMultiplier;
    }

    #endregion

    #region Visual Effects

    private void UpdateVisualEffects()
    {
        UpdateTrailCache();
        UpdateTrails();
    }

    private void UpdateTrailCache()
    {
        cache ??= Enumerable.Repeat(Projectile.Center, TRAIL_LENGTH).ToList();

        cache.Add(Projectile.Center);
        if (cache.Count > TRAIL_LENGTH)
            cache.RemoveAt(0);
    }

    private void UpdateTrails()
    {
        var color = new Color(70, 70, 70);
        float chargeIntensity = 1f + ChargeLevel * 0.5f;

        trail ??= new Trail(Main.instance.GraphicsDevice, TRAIL_LENGTH,
            new RoundedTip(TRAIL_TIP_SIZE),
            factor => factor * 10 * chargeIntensity,
            factor => GetTrailColor(factor, color, 0.15f));

        trail.Positions = [.. cache];
        trail.NextPosition = Projectile.Center;
    }

    private Color GetTrailColor(Vector2 factor, Color baseColor, float baseIntensity)
    {
        if (factor.X >= 0.98f) return Color.Transparent;
        float intensity = (baseIntensity + ChargeLevel * 0.2f) * MathF.Pow(factor.X, 2);
        return baseColor * intensity;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("LightningTrail").Value;
        if (effect == null) return;

        SetupShaderParams(effect);
        trail?.Render(effect);

        effect.Parameters["pixelation"]?.SetValue(6f);
        trail2?.Render(effect);
    }

    private static void SetupShaderParams(Effect effect)
    {
        var transform = Matrix.CreateTranslation(-Main.screenPosition.ToVector3()) *
                       Main.GameViewMatrix.TransformationMatrix *
                       Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

        effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.04f);
        effect.Parameters["repeats"]?.SetValue(8f);
        effect.Parameters["pixelation"]?.SetValue(12f);
        effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        effect.Parameters["transformMatrix"]?.SetValue(transform);
        effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}BeamTrail").Value);
    }

    #endregion

    #region Collision & Damage

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        var impactIntensity = 0;
        var bounceFactor = CurrentAIState switch
        {
            AIState.LaunchingForward => 0.5f,
            _ => 0.3f
        };

        // Handle X collision
        if (MathF.Abs(oldVelocity.X - Projectile.velocity.X) > 0.1f)
        {
            if (MathF.Abs(oldVelocity.X) > 4f) impactIntensity = 1;
            Projectile.velocity.X = -oldVelocity.X * bounceFactor;
            CollisionCounter++;
        }

        // Handle Y collision
        if (MathF.Abs(oldVelocity.Y - Projectile.velocity.Y) > 0.1f)
        {
            if (MathF.Abs(oldVelocity.Y) > 4f) impactIntensity = 1;
            Projectile.velocity.Y = -oldVelocity.Y * bounceFactor;
            CollisionCounter++;
        }

        if (CurrentAIState == AIState.LaunchingForward)
            HandleLaunchCollision(oldVelocity, ref impactIntensity);

        if (impactIntensity > 0)
        {
            Projectile.netUpdate = true;
            for (int i = 0; i < impactIntensity; i++)
                Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);

            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        }

        if (CurrentAIState != AIState.Spinning && CollisionCounter >= MAX_COLLISION_COUNT)
        {
            CurrentAIState = AIState.ForcedRetracting;
            Projectile.netUpdate = true;
        }

        return false;
    }

    private void HandleLaunchCollision(Vector2 velocity, ref int impactIntensity)
    {
        Projectile.localNPCHitCooldown = 10;
        Projectile.netUpdate = true;

        var scanStart = Projectile.TopLeft.ToTileCoordinates();
        var scanEnd = Projectile.BottomRight.ToTileCoordinates();
        impactIntensity = 2;

        Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanStart, ref scanEnd, Projectile.width, out var causedShockwaves);
        Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
        Projectile.position -= velocity;

        if (ChargeLevel == MAX_CHARGE_LEVEL)
            SpawnAcornExplosion(Projectile.Center);
    }

    public override bool? CanDamage() =>
        CurrentAIState == AIState.Spinning && SpinningStateTimer <= 12f ? false : null;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CurrentAIState == AIState.Spinning)
        {
            var mountedCenter = Main.player[Projectile.owner].MountedCenter;
            var toTarget = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
            toTarget.Y /= 0.8f; // Account for flattened ellipse
            return toTarget.LengthSquared() <= HitRadius * HitRadius;
        }
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (ChargeLevel == MAX_CHARGE_LEVEL)
            SpawnAcornExplosion(target.Center);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}HewerReturn")
        {
            MaxInstances = 1,
            Volume = MathHelper.Lerp(0.4f, 0.7f, ChargeLevel / (float)MAX_CHARGE_LEVEL),
            Pitch = MathHelper.Lerp(-0.9f, -0.6f, ChargeLevel / (float)MAX_CHARGE_LEVEL),
        }, Projectile.position);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        float damageMultiplier = CurrentAIState switch
        {
            AIState.Spinning => 1.1f + ChargeLevel * 0.2f,
            AIState.LaunchingForward or AIState.Retracting => 1.3f + ChargeLevel * 0.3f,
            _ => 1f
        };

        modifiers.SourceDamage *= damageMultiplier;
        modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Main.player[Projectile.owner].Center.X);

        float knockbackMultiplier = CurrentAIState switch
        {
            AIState.Spinning => 0.25f * ChargeMultiplier,
            _ => ChargeMultiplier
        };

        modifiers.Knockback *= knockbackMultiplier;
    }

    #endregion

    #region Drawing & Rendering

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Projectile.type].Value;
        var drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
        var drawColor = Projectile.GetAlpha(lightColor);
        var drawOrigin = texture.Size() * 0.5f;
        var spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        DrawChain();
        DrawAfterImages(lightColor);

        // Draw main projectile
        Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation,
            drawOrigin, Projectile.scale, spriteEffects, 0);

        // Draw charge flash when fully charged and spinning
        if (CurrentAIState == AIState.Spinning && ChargeLevel >= MAX_CHARGE_LEVEL)
        {
            float flashCycle = (Main.GameUpdateCount % 30) / 30f;
            float flashOpacity = flashCycle < 0.15f ? MathF.Sin(flashCycle * MathHelper.Pi * 6.67f) : 0f;

            if (flashOpacity > 0f)
            {
                var flashColor = Color.Lerp(new Color(139, 69, 19), Color.White, 0.8f).ToVector4();
                flashColor.W = 0f; // Make it additive

                Main.EntitySpriteDraw(texture, drawPos, null, new Color(flashColor) * flashOpacity * 1.5f,
                    Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0);
            }
        }

        return false;
    }

    private void DrawChain()
    {
        var playerArmPos = Main.GetPlayerArmPosition(Projectile);
        playerArmPos.Y -= Main.player[Projectile.owner].gfxOffY;

        var chainTexture = ModContent.Request<Texture2D>(CHAIN_TEXTURE_PATH);
        var chainOrigin = chainTexture.Size() * 0.5f;
        var toPlayer = playerArmPos.MoveTowards(Projectile.Center, 4f) - Projectile.Center;
        var chainDir = Vector2.Normalize(toPlayer);
        var chainRotation = chainDir.ToRotation() + MathHelper.PiOver2;
        var segmentLength = chainTexture.Height();

        if (segmentLength == 0) segmentLength = 10;

        var chainPos = Projectile.Center;
        var remainingLength = toPlayer.Length() + segmentLength * 0.5f;
        int segmentCount = 0;

        while (remainingLength > 0f)
        {
            var chainColor = Lighting.GetColor((int)(chainPos.X / 16), (int)(chainPos.Y / 16));

            // Enhance color for charged segments near projectile
            if (segmentCount < 2 && ChargeLevel > 0)
                chainColor = Color.Lerp(Color.White, new Color(139, 69, 19), ChargeLevel * 0.3f);
            else if (segmentCount < 4 && ChargeLevel > 0)
            {
                byte minValue = (byte)(140 + ChargeLevel * 20);
                chainColor.R = Math.Max(chainColor.R, minValue);
                chainColor.G = Math.Max(chainColor.G, minValue);
                chainColor.B = Math.Max(chainColor.B, minValue);
            }

            Main.spriteBatch.Draw(chainTexture.Value, chainPos - Main.screenPosition, null,
                chainColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);

            chainPos += chainDir * segmentLength;
            segmentCount++;
            remainingLength -= segmentLength;
        }
    }

    private void DrawAfterImages(Color lightColor)
    {
        if (CurrentAIState != AIState.LaunchingForward) return;

        var texture = TextureAssets.Projectile[Projectile.type].Value;
        var drawOrigin = texture.Size() * 0.5f;
        var spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        for (int k = 0; k < Math.Min(Projectile.oldPos.Length, StateTimer); k++)
        {
            var drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            float opacity = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
            var color = Projectile.GetAlpha(lightColor) * opacity;

            if (ChargeLevel > 0)
                color = Color.Lerp(color, new Color(139, 69, 19), ChargeLevel * 0.2f);

            float scale = Projectile.scale - k / (float)Projectile.oldPos.Length / 3;
            Main.spriteBatch.Draw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, scale, spriteEffects, 0f);
        }
    }

    #endregion

    #region Helper Methods

    private void UpdateProjectileTransform(Vector2 mountedCenter)
    {
        Projectile.direction = Math.Sign(Projectile.velocity.X);
        Projectile.spriteDirection = Projectile.direction;
        Projectile.rotation = (mountedCenter - Projectile.Center).ToRotation() + MathHelper.PiOver2;
    }

    private void UpdatePlayerAnimation(Player player, Vector2 mountedCenter)
    {
        Projectile.timeLeft = 2;
        player.heldProj = Projectile.whoAmI;
        player.SetDummyItemTime(2);
        player.itemRotation = (Projectile.Center - mountedCenter).ToRotation();

        if (Projectile.Center.X < mountedCenter.X)
            player.itemRotation += MathHelper.Pi;

        player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
    }

    private void SpawnAcornExplosion(Vector2 position)
    {
        if (Main.myPlayer != Projectile.owner) return;

        CameraSystem.shake = 15;
        SoundEngine.PlaySound(SoundID.Item14, position);

        const int acornCount = 7;
        const float arcSpread = MathHelper.Pi * 0.6f;

        var baseDir = Projectile.velocity.Length() > 1f ?
            Vector2.Normalize(Projectile.velocity) :
            Vector2.Normalize(Main.MouseWorld - Projectile.Center);

        for (int i = 0; i < acornCount; i++)
        {
            float angle = MathHelper.Lerp(-arcSpread * 0.5f, arcSpread * 0.5f, i / (float)(acornCount - 1));
            var velocity = Vector2.Transform(baseDir, Matrix.CreateRotationZ(angle)) * Main.rand.NextFloat(8f, 14f);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity,
                ModContent.ProjectileType<AcornProj>(), (int)(Projectile.damage * 0.8f),
                Projectile.knockBack * 0.6f, Projectile.owner);
        }
    }

    #endregion
}