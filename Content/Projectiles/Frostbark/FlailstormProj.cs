using Reverie.Core.Graphics;
using Reverie.Core.Graphics.Interfaces;
using Reverie.Core.Loaders;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.Projectiles.Frostbark;

public class FlailstormProj : ModProjectile, IDrawPrimitive
{
    private const string ChainTexturePath = "Reverie/Assets/Textures/Projectiles/Frostbark/FlailstormChain";
    private const string ChainTextureExtraPath = ChainTexturePath;

    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;

    // Charging system constants
    private const float ChargeLevel1Time = 60f;  // 1 second
    private const float ChargeLevel2Time = 120f; // 2 seconds  
    private const float ChargeLevel3Time = 180f; // 3 seconds
    private const int IceCloudSpawnRate = 15;    // Spawn ice cloud every 15 frames when charging
    private float lastIceCloudSpawn = 0f;

    // Charge properties
    private int ChargeLevel => SpinningStateTimer < ChargeLevel1Time ? 0 :
                              SpinningStateTimer < ChargeLevel2Time ? 1 :
                              SpinningStateTimer < ChargeLevel3Time ? 2 : 3;

    private float ChargeMultiplier => 1f + ChargeLevel * 0.5f; // 1x, 1.5x, 2x, 2.5x knockback

    private enum AIState
    {
        Spinning,
        LaunchingForward,
        Retracting,
        ForcedRetracting,
        Dropping
    }

    private AIState CurrentAIState
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }
    public ref float StateTimer => ref Projectile.ai[1];
    public ref float CollisionCounter => ref Projectile.localAI[0];
    public ref float SpinningStateTimer => ref Projectile.localAI[1];

    private const int LaunchTimeLimit = 18;
    private const float LaunchSpeed = 14f;
    private const float MaxLaunchLength = 800f;
    private const float RetractAcceleration = 3f;
    private const float MaxRetractSpeed = 10f;
    private const float ForcedRetractAcceleration = 6f;
    private const float MaxForcedRetractSpeed = 15f;
    private const int SpinHitCooldown = 20;
    private const int MovingHitCooldown = 10;

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
        Projectile.localNPCHitCooldown = 30;
    }

    public override void AI()
    {
        ManageCaches();
        ManageTrail();
        var player = Main.player[Projectile.owner];

        if (!player.active || player.dead || player.noItems || player.CCed || Vector2.Distance(Projectile.Center, player.Center) > 900f)
        {
            Projectile.Kill();
            return;
        }

        if (Main.myPlayer == Projectile.owner && Main.mapFullscreen)
        {
            Projectile.Kill();
            return;
        }

        var mountedCenter = player.MountedCenter;
        var meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);

        switch (CurrentAIState)
        {
            case AIState.Spinning:
                HandleSpinningState(player, mountedCenter);
                break;
            case AIState.LaunchingForward:
                HandleLaunchingForwardState(player, mountedCenter, meleeSpeedMultiplier);
                break;
            case AIState.Retracting:
                HandleRetractingState(player, mountedCenter, meleeSpeedMultiplier);
                break;
            case AIState.ForcedRetracting:
                HandleForcedRetractingState(player, mountedCenter, meleeSpeedMultiplier);
                break;
            case AIState.Dropping:
                HandleDroppingState(player, mountedCenter);
                break;
        }

        HandleProjectileRotation(mountedCenter);
        HandlePlayerAnimation(player, mountedCenter);
        SpawnDust();
    }

    private void HandleSpinningState(Player player, Vector2 mountedCenter)
    {
        if (Projectile.owner == Main.myPlayer)
        {
            var unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
            player.ChangeDir((unitVectorTowardsMouse.X > 0f).ToDirectionInt());
            if (!player.channel)
            {
                CurrentAIState = AIState.LaunchingForward;
                StateTimer = 0f;
                Projectile.velocity = unitVectorTowardsMouse * LaunchSpeed + player.velocity;
                Projectile.Center = mountedCenter;
                Projectile.netUpdate = true;
                Projectile.ResetLocalNPCHitImmunity();
                Projectile.localNPCHitCooldown = MovingHitCooldown;
                return;
            }
        }

        SpinningStateTimer += 1f;

        // Spawn ice clouds while charging
        if (SpinningStateTimer > 30f && SpinningStateTimer - lastIceCloudSpawn >= IceCloudSpawnRate)
        {
            SpawnIceCloud(player, mountedCenter);
            lastIceCloudSpawn = SpinningStateTimer;
        }

        // Visual and audio feedback for charge levels
        if (SpinningStateTimer == ChargeLevel1Time || SpinningStateTimer == ChargeLevel2Time || SpinningStateTimer == ChargeLevel3Time)
        {
            // Play charge up sound
            SoundEngine.PlaySound(SoundID.Item30, Projectile.position);

            // Spawn visual effect
            for (int i = 0; i < 10; i++)
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.IceTorch, 0f, 0f, 150, default, 1.5f);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.noGravity = true;
            }
        }

        var offsetFromPlayer = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (SpinningStateTimer / 60f) * player.direction);
        offsetFromPlayer.Y *= 0.8f;
        if (offsetFromPlayer.Y * player.gravDir > 0f)
        {
            offsetFromPlayer.Y *= 0.5f;
        }

        // Increase spin radius slightly when charged
        float spinRadius = 30f + ChargeLevel * 5f;
        Projectile.Center = mountedCenter + offsetFromPlayer * spinRadius + new Vector2(0, player.gfxOffY);
        Projectile.velocity = Vector2.Zero;
        Projectile.localNPCHitCooldown = SpinHitCooldown;
    }

    private void SpawnIceCloud(Player player, Vector2 mountedCenter)
    {
        if (Main.myPlayer != Projectile.owner) return;

        // Spawn ice cloud projectile around the flail
        var cloudPosition = Projectile.Center + Main.rand.NextVector2Circular(40f, 40f);
        var cloudVelocity = Main.rand.NextVector2Circular(2f, 2f);

        // You'll need to create an IceCloudProj projectile
        // Projectile.NewProjectile(Projectile.GetSource_FromThis(), cloudPosition, cloudVelocity, 
        //     ModContent.ProjectileType<IceCloudProj>(), Projectile.damage / 3, 0f, player.whoAmI);
    }

    private void HandleLaunchingForwardState(Player player, Vector2 mountedCenter, float meleeSpeedMultiplier)
    {
        var shouldSwitchToRetracting = StateTimer++ >= LaunchTimeLimit;
        shouldSwitchToRetracting |= Projectile.Distance(mountedCenter) >= MaxLaunchLength;
        if (player.controlUseItem)
        {
            CurrentAIState = AIState.Dropping;
            StateTimer = 0f;
            Projectile.netUpdate = true;
            Projectile.velocity *= 0.2f;
        }
        else if (shouldSwitchToRetracting)
        {
            CurrentAIState = AIState.Retracting;
            StateTimer = 0f;
            Projectile.netUpdate = true;
            Projectile.velocity *= 0.3f;
        }
        player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
        Projectile.localNPCHitCooldown = MovingHitCooldown;
    }

    private void HandleRetractingState(Player player, Vector2 mountedCenter, float meleeSpeedMultiplier)
    {
        var unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
        if (Projectile.Distance(mountedCenter) <= MaxRetractSpeed * meleeSpeedMultiplier)
        {
            Projectile.Kill();
            return;
        }
        if (player.controlUseItem)
        {
            CurrentAIState = AIState.Dropping;
            StateTimer = 0f;
            Projectile.netUpdate = true;
            Projectile.velocity *= 0.2f;
        }
        else
        {
            Projectile.velocity *= 0.98f;
            Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * MaxRetractSpeed * meleeSpeedMultiplier, RetractAcceleration * meleeSpeedMultiplier);
            player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
        }
    }

    private void HandleForcedRetractingState(Player player, Vector2 mountedCenter, float meleeSpeedMultiplier)
    {
        Projectile.tileCollide = false;
        var unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
        if (Projectile.Distance(mountedCenter) <= MaxForcedRetractSpeed * meleeSpeedMultiplier)
        {
            Projectile.Kill();
            return;
        }
        Projectile.velocity *= 0.98f;
        Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * MaxForcedRetractSpeed * meleeSpeedMultiplier, ForcedRetractAcceleration * meleeSpeedMultiplier);
        var target = Projectile.Center + Projectile.velocity;
        var value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
        if (Vector2.Dot(unitVectorTowardsPlayer, value) < 0f)
        {
            Projectile.Kill();
            return;
        }
        player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
    }

    private void HandleDroppingState(Player player, Vector2 mountedCenter)
    {
        if (!player.controlUseItem || Projectile.Distance(mountedCenter) > MaxLaunchLength + 160f)
        {
            CurrentAIState = AIState.ForcedRetracting;
            StateTimer = 0f;
            Projectile.netUpdate = true;
        }
        else
        {
            Projectile.velocity.Y += 0.8f;
            Projectile.velocity.X *= 0.95f;
            player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
        }
    }

    private void HandleProjectileRotation(Vector2 mountedCenter)
    {
        Projectile.direction = (Projectile.velocity.X > 0f).ToDirectionInt();
        Projectile.spriteDirection = Projectile.direction;
        Projectile.rotation = CurrentAIState == AIState.Dropping
            ? Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f
            : Projectile.DirectionTo(mountedCenter).ToRotation() + MathHelper.PiOver2;
    }

    private void HandlePlayerAnimation(Player player, Vector2 mountedCenter)
    {
        Projectile.timeLeft = 2;
        player.heldProj = Projectile.whoAmI;
        player.SetDummyItemTime(2);
        player.itemRotation = Projectile.DirectionFrom(mountedCenter).ToRotation();
        if (Projectile.Center.X < mountedCenter.X)
        {
            player.itemRotation += (float)Math.PI;
        }
        player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
    }

    private void SpawnDust()
    {
        // Spawn more dust when charged
        int dustFrequency = CurrentAIState == AIState.LaunchingForward ? 3 : 7;
        if (CurrentAIState == AIState.Spinning && ChargeLevel > 0)
        {
            dustFrequency = Math.Max(1, dustFrequency - ChargeLevel);
        }

        if (Main.rand.NextBool(dustFrequency))
        {
            var dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                DustID.FrostDaggerfish, 0f, 0f, 150, default, 1.3f + ChargeLevel * 0.3f)];

            if (ChargeLevel > 0)
            {
                dust.noGravity = true;
                dust.velocity *= 1f + ChargeLevel * 0.5f;
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        var defaultLocalNPCHitCooldown = 10;
        var impactIntensity = 0;
        var velocity = Projectile.velocity;
        var bounceFactor = 0.3f;

        if (CurrentAIState == AIState.LaunchingForward)
        {
            bounceFactor = 0.5f;
        }

        if (CurrentAIState == AIState.Dropping)
        {
            bounceFactor = 0.1f;
        }

        if (oldVelocity.X != Projectile.velocity.X)
        {
            if (Math.Abs(oldVelocity.X) > 4f)
            {
                impactIntensity = 1;
            }

            Projectile.velocity.X = (0f - oldVelocity.X) * bounceFactor;
            CollisionCounter += 1f;
        }

        if (oldVelocity.Y != Projectile.velocity.Y)
        {
            if (Math.Abs(oldVelocity.Y) > 4f)
            {
                impactIntensity = 1;
            }

            Projectile.velocity.Y = (0f - oldVelocity.Y) * bounceFactor;
            CollisionCounter += 1f;
        }

        if (CurrentAIState == AIState.LaunchingForward)
        {
            Projectile.localNPCHitCooldown = defaultLocalNPCHitCooldown;
            Projectile.netUpdate = true;
            var scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
            var scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
            impactIntensity = 2;
            Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out var causedShockwaves);
            Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
            Projectile.position -= velocity;

            // Spawn ice explosion on charged tile collision
            if (ChargeLevel > 0)
            {
                SpawnIceExplosion(Projectile.Center);
            }
        }

        if (impactIntensity > 0)
        {
            Projectile.netUpdate = true;
            for (var i = 0; i < impactIntensity; i++)
            {
                Collision.HitTiles(Projectile.position, velocity, Projectile.width, Projectile.height);
            }

            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        }

        if (CurrentAIState != AIState.Spinning && CurrentAIState != AIState.Dropping && CollisionCounter >= 10f)
        {
            CurrentAIState = AIState.ForcedRetracting;
            Projectile.netUpdate = true;
        }
        return false;
    }

    public override bool? CanDamage()
    {
        if (CurrentAIState == AIState.Spinning && SpinningStateTimer <= 12f)
        {
            return false;
        }
        return base.CanDamage();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CurrentAIState == AIState.Spinning)
        {
            var mountedCenter = Main.player[Projectile.owner].MountedCenter;
            var shortestVectorFromPlayerToTarget = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
            shortestVectorFromPlayerToTarget.Y /= 0.8f;
            var hitRadius = 55f + ChargeLevel * 10f; // Larger hitbox when charged
            return shortestVectorFromPlayerToTarget.Length() <= hitRadius;
        }
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}HewerReturn")
        {
            MaxInstances = 1,
            Volume = 0.4f + ChargeLevel * 0.1f,
            Pitch = -0.9f + ChargeLevel * 0.1f,
        }, Projectile.position);

        // Spawn ice explosion when charged
        if (ChargeLevel > 0)
        {
            SpawnIceExplosion(target.Center);
        }
    }

    private void SpawnIceExplosion(Vector2 position)
    {
        if (Main.myPlayer != Projectile.owner) return;

        // Spawn ice explosion projectiles based on charge level
        int explosionCount = 3 + ChargeLevel * 2; // 3, 5, 7, or 9 projectiles
        float explosionRadius = 80f + ChargeLevel * 20f;

        for (int i = 0; i < explosionCount; i++)
        {
            var angle = (MathHelper.TwoPi / explosionCount) * i;
            var velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;

            // You'll need to create an IceExplosionProj projectile
            // Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity,
            //     ModContent.ProjectileType<IceExplosionProj>(), Projectile.damage, Projectile.knockBack, 
            //     Projectile.owner);
        }

        // Visual and audio effects
        SoundEngine.PlaySound(SoundID.Item27, position);

        for (int i = 0; i < 20 + ChargeLevel * 10; i++)
        {
            var dust = Dust.NewDustDirect(position - Vector2.One * 20f, 40, 40,
                DustID.IceTorch, 0f, 0f, 150, default, 1.5f + ChargeLevel * 0.5f);
            dust.velocity = Main.rand.NextVector2Circular(5f + ChargeLevel * 2f, 5f + ChargeLevel * 2f);
            dust.noGravity = true;
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}HewerReturn")
        {
            MaxInstances = 1,
            Volume = 0.4f + ChargeLevel * 0.1f,
            Pitch = -0.9f + ChargeLevel * 0.1f,
        }, Projectile.position);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (CurrentAIState == AIState.Spinning)
            modifiers.SourceDamage *= 1.1f + ChargeLevel * 0.2f; // Increased damage when charged

        else if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Retracting)
            modifiers.SourceDamage *= 1.3f + ChargeLevel * 0.3f; // Even more damage when charged

        modifiers.HitDirectionOverride = (Main.player[Projectile.owner].Center.X < target.Center.X).ToDirectionInt();

        if (CurrentAIState == AIState.Spinning)
            modifiers.Knockback *= 0.25f * ChargeMultiplier; // Charge affects knockback

        else if (CurrentAIState == AIState.Dropping)
            modifiers.Knockback *= 0.5f * ChargeMultiplier;
        else
            modifiers.Knockback *= ChargeMultiplier; // Apply charge multiplier to other states
    }

    private void ManageCaches()
    {
        var pos = Projectile.Center;

        if (cache == null)
        {
            cache = [];
            for (var i = 0; i < 25; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);
        while (cache.Count > 25)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var pos = Projectile.Center;
        Color iceColor = new Color(120, 200, 255);

        // Scale trail width based on charge level
        float chargeTrailMultiplier = 1f + ChargeLevel * 1.5f; // 1x, 1.5x, 2x, 2.5x width

        trail ??= new Trail(Main.instance.GraphicsDevice, 25, new RoundedTip(16), factor => factor * 30 * chargeTrailMultiplier, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            // More intense color when charged
            float chargeIntensity = 0.6f + ChargeLevel * 0.2f;
            return iceColor * chargeIntensity * (float)Math.Pow(factor.X, 2);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 25, new RoundedTip(16), factor => factor * 20 * chargeTrailMultiplier, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            // Brighter center when charged
            float chargeIntensity = 0.7f + ChargeLevel * 0.2f;
            return Color.Lerp(iceColor, Color.White, 0.6f + ChargeLevel * 0.1f) * chargeIntensity * (float)Math.Pow(factor.X, 2);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos;
        trail2.NextPosition = pos;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("LightningTrail").Value;

        if (effect != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.04f);
            effect.Parameters["repeats"]?.SetValue(8f);
            effect.Parameters["pixelation"]?.SetValue(12f);
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
        var playerArmPosition = Main.GetPlayerArmPosition(Projectile);
        playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;

        var chainTexture = ModContent.Request<Texture2D>(ChainTexturePath);
        var chainTextureExtra = ModContent.Request<Texture2D>(ChainTextureExtraPath);
        Rectangle? chainSourceRectangle = null;
        var chainHeightAdjustment = 0f;

        var chainOrigin = chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Size() / 2f : chainTexture.Size() / 2f;
        var chainDrawPosition = Projectile.Center;
        var vectorFromProjectileToPlayerArms = playerArmPosition.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
        var unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero);
        var chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
        if (chainSegmentLength == 0)
        {
            chainSegmentLength = 10;
        }
        var chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
        var chainCount = 0;
        var chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() + chainSegmentLength / 2f;

        while (chainLengthRemainingToDraw > 0f)
        {
            var chainDrawColor = Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));

            var chainTextureToDraw = chainTexture;
            if (chainCount >= 4)
            {

            }
            else if (chainCount >= 2)
            {
                chainTextureToDraw = chainTextureExtra;
                byte minValue = (byte)(140 + ChargeLevel * 20); // Brighter when charged
                if (chainDrawColor.R < minValue)
                    chainDrawColor.R = minValue;

                if (chainDrawColor.G < minValue)
                    chainDrawColor.G = minValue;

                if (chainDrawColor.B < minValue)
                    chainDrawColor.B = minValue;
            }
            else
            {
                chainTextureToDraw = chainTextureExtra;
                // Add ice glow when charged
                chainDrawColor = ChargeLevel > 0 ?
                    Color.Lerp(Color.White, new Color(120, 200, 255), ChargeLevel * 0.3f) :
                    Color.White;
            }

            Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);

            chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
            chainCount++;
            chainLengthRemainingToDraw -= chainSegmentLength;
        }

        if (CurrentAIState == AIState.LaunchingForward)
        {
            var projectileTexture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(projectileTexture.Width * 0.5f, Projectile.height * 0.5f);
            var spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            for (var k = 0; k < Projectile.oldPos.Length && k < StateTimer; k++)
            {
                var drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                var color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                // Add charge glow
                if (ChargeLevel > 0)
                {
                    color = Color.Lerp(color, new Color(120, 200, 255), ChargeLevel * 0.2f);
                }

                Main.spriteBatch.Draw(projectileTexture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, spriteEffects, 0f);
            }
        }
        return true;
    }
}