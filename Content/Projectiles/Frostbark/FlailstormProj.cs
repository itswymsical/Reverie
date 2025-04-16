using Terraria.Audio;
using Terraria.GameContent;


namespace Reverie.Content.Projectiles.Frostbark;

public class FlailstormProj : ModProjectile
{
    private const string ChainTexturePath = "Reverie/Assets/Textures/Projectiles/Frostbark/FlailstormChain";
    private const string ChainTextureExtraPath = ChainTexturePath;

    private enum AIState
    {
        Spinning,
        LaunchingForward,
        Retracting,
        UnusedState,
        ForcedRetracting,
        Ricochet,
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
            case AIState.Ricochet:
                HandleRicochetState(player);
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
        var offsetFromPlayer = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (SpinningStateTimer / 60f) * player.direction);
        offsetFromPlayer.Y *= 0.8f;
        if (offsetFromPlayer.Y * player.gravDir > 0f)
        {
            offsetFromPlayer.Y *= 0.5f;
        }
        Projectile.Center = mountedCenter + offsetFromPlayer * 30f + new Vector2(0, player.gfxOffY);
        Projectile.velocity = Vector2.Zero;
        Projectile.localNPCHitCooldown = SpinHitCooldown;
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

    private void HandleRicochetState(Player player)
    {
        if (StateTimer++ >= LaunchTimeLimit + 5)
        {
            CurrentAIState = AIState.Dropping;
            StateTimer = 0f;
            Projectile.netUpdate = true;
        }
        else
        {
            Projectile.localNPCHitCooldown = MovingHitCooldown;
            Projectile.velocity.Y += 0.6f;
            Projectile.velocity.X *= 0.95f;
            player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
        }
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
        Projectile.rotation = CurrentAIState == AIState.Ricochet || CurrentAIState == AIState.Dropping
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
        if (Main.rand.NextBool(CurrentAIState == AIState.LaunchingForward ? 3 : 7))
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostDaggerfish, 0f, 0f, 150, default, 1.3f);
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        var defaultLocalNPCHitCooldown = 10;
        var impactIntensity = 0;
        var velocity = Projectile.velocity;
        var bounceFactor = 0.4f;
        if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Ricochet)
        {
            bounceFactor = 0.9f;
        }

        if (CurrentAIState == AIState.Dropping)
        {
            bounceFactor = 0.3f;
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
            CurrentAIState = AIState.Ricochet;
            Projectile.localNPCHitCooldown = defaultLocalNPCHitCooldown;
            Projectile.netUpdate = true;
            var scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
            var scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
            impactIntensity = 2;
            Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out var causedShockwaves);
            Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
            Projectile.position -= velocity;
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

        if (CurrentAIState != AIState.UnusedState && CurrentAIState != AIState.Spinning && CurrentAIState != AIState.Ricochet && CurrentAIState != AIState.Dropping && CollisionCounter >= 10f)
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
            var hitRadius = 55f;
            return shortestVectorFromPlayerToTarget.Length() <= hitRadius;
        }
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}HewerReturn")
        {
            MaxInstances = 1,
            Volume = 0.4f,
            Pitch = -0.9f,
        }, Projectile.position);
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}HewerReturn")
        {
            MaxInstances = 1,
            Volume = 0.4f,
            Pitch = -0.9f,
        }, Projectile.position);
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (CurrentAIState == AIState.Spinning)
            modifiers.SourceDamage *= 1.1f;

        else if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Retracting)
            modifiers.SourceDamage *= 1.3f;

        modifiers.HitDirectionOverride = (Main.player[Projectile.owner].Center.X < target.Center.X).ToDirectionInt();

        if (CurrentAIState == AIState.Spinning)
            modifiers.Knockback *= 0.25f;

        else if (CurrentAIState == AIState.Dropping)
            modifiers.Knockback *= 0.5f;
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
                byte minValue = 140;
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
                chainDrawColor = Color.White;
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
                Main.spriteBatch.Draw(projectileTexture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, spriteEffects, 0f);
            }
        }
        return true;
    }
}