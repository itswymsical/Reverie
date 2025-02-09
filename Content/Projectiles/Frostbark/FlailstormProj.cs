using ReLogic.Content;
using static Reverie.Reverie;

using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.Projectiles.Frostbark;

public class FlailstormProj : ModProjectile
{
    private const string CHAIN_TEXTURE_PATH = $"Reverie/Assets/Textures/Projectiles/Frostbark/FlailstormChain";
    private const string CHAIN_TEXTURE_EXTRA_PATH = CHAIN_TEXTURE_PATH;

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

    private const int LAUNCH_TIME_LIMIT = 18;
    private const float LAUNCH_SPEED = 14f;
    private const float MAX_LAUNCH_LENGTH = 800f;
    private const float RETRACT_ACCELERATION = 3f;
    private const float MAX_RETRACT_SPEED = 10f;
    private const float FORCED_RETRACT_ACCELERATION = 6f;
    private const float MAX_FORCED_RETRACT_SPEED = 15f;
    private const int SPIN_HIT_COOLDOWN = 20;
    private const int MOVING_HIT_COOLDOWN = 10;

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
        Player player = Main.player[Projectile.owner];

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

        Vector2 mountedCenter = player.MountedCenter;
        float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);

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
            Vector2 unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
            player.ChangeDir((unitVectorTowardsMouse.X > 0f).ToDirectionInt());
            if (!player.channel)
            {
                CurrentAIState = AIState.LaunchingForward;
                StateTimer = 0f;
                Projectile.velocity = unitVectorTowardsMouse * LAUNCH_SPEED + player.velocity;
                Projectile.Center = mountedCenter;
                Projectile.netUpdate = true;
                Projectile.ResetLocalNPCHitImmunity();
                Projectile.localNPCHitCooldown = MOVING_HIT_COOLDOWN;
                return;
            }
        }
        SpinningStateTimer += 1f;
        Vector2 offsetFromPlayer = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (SpinningStateTimer / 60f) * player.direction);
        offsetFromPlayer.Y *= 0.8f;
        if (offsetFromPlayer.Y * player.gravDir > 0f)
        {
            offsetFromPlayer.Y *= 0.5f;
        }
        Projectile.Center = mountedCenter + offsetFromPlayer * 30f + new Vector2(0, player.gfxOffY);
        Projectile.velocity = Vector2.Zero;
        Projectile.localNPCHitCooldown = SPIN_HIT_COOLDOWN;
    }

    private void HandleLaunchingForwardState(Player player, Vector2 mountedCenter, float meleeSpeedMultiplier)
    {
        bool shouldSwitchToRetracting = StateTimer++ >= LAUNCH_TIME_LIMIT;
        shouldSwitchToRetracting |= Projectile.Distance(mountedCenter) >= MAX_LAUNCH_LENGTH;
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
        Projectile.localNPCHitCooldown = MOVING_HIT_COOLDOWN;
    }

    private void HandleRetractingState(Player player, Vector2 mountedCenter, float meleeSpeedMultiplier)
    {
        Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
        if (Projectile.Distance(mountedCenter) <= MAX_RETRACT_SPEED * meleeSpeedMultiplier)
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
            Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * MAX_RETRACT_SPEED * meleeSpeedMultiplier, RETRACT_ACCELERATION * meleeSpeedMultiplier);
            player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
        }
    }

    private void HandleForcedRetractingState(Player player, Vector2 mountedCenter, float meleeSpeedMultiplier)
    {
        Projectile.tileCollide = false;
        Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
        if (Projectile.Distance(mountedCenter) <= MAX_FORCED_RETRACT_SPEED * meleeSpeedMultiplier)
        {
            Projectile.Kill();
            return;
        }
        Projectile.velocity *= 0.98f;
        Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * MAX_FORCED_RETRACT_SPEED * meleeSpeedMultiplier, FORCED_RETRACT_ACCELERATION * meleeSpeedMultiplier);
        Vector2 target = Projectile.Center + Projectile.velocity;
        Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
        if (Vector2.Dot(unitVectorTowardsPlayer, value) < 0f)
        {
            Projectile.Kill();
            return;
        }
        player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
    }

    private void HandleRicochetState(Player player)
    {
        if (StateTimer++ >= LAUNCH_TIME_LIMIT + 5)
        {
            CurrentAIState = AIState.Dropping;
            StateTimer = 0f;
            Projectile.netUpdate = true;
        }
        else
        {
            Projectile.localNPCHitCooldown = MOVING_HIT_COOLDOWN;
            Projectile.velocity.Y += 0.6f;
            Projectile.velocity.X *= 0.95f;
            player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
        }
    }

    private void HandleDroppingState(Player player, Vector2 mountedCenter)
    {
        if (!player.controlUseItem || Projectile.Distance(mountedCenter) > MAX_LAUNCH_LENGTH + 160f)
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
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostDaggerfish, 0f, 0f, 150, default(Color), 1.3f);
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        int defaultLocalNPCHitCooldown = 10;
        int impactIntensity = 0;
        Vector2 velocity = Projectile.velocity;
        float bounceFactor = 0.4f;
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
            Point scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
            Point scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
            impactIntensity = 2;
            Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out bool causedShockwaves);
            Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
            Projectile.position -= velocity;
        }

        if (impactIntensity > 0)
        {
            Projectile.netUpdate = true;
            for (int i = 0; i < impactIntensity; i++)
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
            Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 shortestVectorFromPlayerToTarget = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
            shortestVectorFromPlayerToTarget.Y /= 0.8f;
            float hitRadius = 55f;
            return shortestVectorFromPlayerToTarget.Length() <= hitRadius;
        }
        return base.Colliding(projHitbox, targetHitbox);
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
        Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);

        playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;

        Asset<Texture2D> chainTexture = ModContent.Request<Texture2D>(CHAIN_TEXTURE_PATH);
        Asset<Texture2D> chainTextureExtra = ModContent.Request<Texture2D>(CHAIN_TEXTURE_EXTRA_PATH);
        Rectangle? chainSourceRectangle = null;
        float chainHeightAdjustment = 0f;

        Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (chainTexture.Size() / 2f);
        Vector2 chainDrawPosition = Projectile.Center;
        Vector2 vectorFromProjectileToPlayerArms = playerArmPosition.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
        Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero);
        float chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
        if (chainSegmentLength == 0)
        {
            chainSegmentLength = 10;
        }
        float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
        int chainCount = 0;
        float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() + chainSegmentLength / 2f;

        while (chainLengthRemainingToDraw > 0f)
        {
            Color chainDrawColor = Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));

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
            Texture2D projectileTexture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(projectileTexture.Width * 0.5f, Projectile.height * 0.5f);
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            for (int k = 0; k < Projectile.oldPos.Length && k < StateTimer; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.spriteBatch.Draw(projectileTexture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, spriteEffects, 0f);
            }
        }
        return true;
    }
}
