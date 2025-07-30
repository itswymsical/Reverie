using Reverie.Utilities;
using Reverie.Core.Cinematics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Reverie.Core.Cinematics.Camera;

namespace Reverie.Content.NPCs.Bosses.KingSlime;

public class NewKingSlime : ModNPC
{
    public override string Texture => $"{TEXTURE_DIRECTORY}NPCs/Bosses/KingSlime/KingSlime";

    private AIState State
    {
        get => (AIState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private ref float Timer => ref NPC.ai[1];

    internal enum AIState
    {
        Strolling,
        Jumping,
        GroundPound,
        Teleporting,
        Despawning,
        ConsumingSlimes
    }

    private Vector2 squishScale = Vector2.One;
    private bool wasGliding = false;
    private ref float JumpPhase => ref NPC.ai[2];
    private ref float StrollCount => ref NPC.ai[3];
    private int targetStrolls = 3;

    private float rotationSway = 0f;
    private float wobbleTimer = 0f;
    private bool isWobbling = false;

    private int groundPoundCount = 0;
    private int targetGroundPounds = 1;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.npcFrameCount[Type] = 6;
    }

    public override void SetDefaults()
    {
        NPC.damage = 12;
        NPC.defense = 14;
        NPC.lifeMax = 900;
        NPC.width = 158;
        NPC.height = 106;
        NPC.aiStyle = -1;
        NPC.value = Item.buyPrice(gold: 4);
        NPC.boss = true;
        NPC.lavaImmune = true;
        NPC.knockBackResist = 0f;
        if (!Main.dedServ)
            Music = MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}GelatinousJoust");
        NPC.HitSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeHit") with { PitchVariance = 0.3f };
        NPC.DeathSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeDeath") with { PitchVariance = 0.2f };
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];

        if (!target.active || target.dead)
        {
            NPC.TargetClosest();
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                State = AIState.Despawning;
                return;
            }
        }
        Timer++;
        HandleSquishScale();
        HandleRotation();
        HandleDustTrail();

        switch (State)
        {
            case AIState.Strolling:
                DoStrolling(target);
                break;
            case AIState.Jumping:
                DoJumping(target);
                break;
            case AIState.GroundPound:
                DoGroundPound(target);
                break;
        }
        NPCUtils.SlopedCollision(NPC);
        NPCUtils.CheckPlatform(NPC, target);
    }

    private void DoStrolling(Player target)
    {
        if (Timer == 1)
        {
            targetStrolls = Main.rand.NextBool() ? 3 : 6;
        }

        float distToPlayer = Vector2.Distance(NPC.Center, target.Center);
        float dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);
        float strollDuration = 54f;
        float progress = Timer / strollDuration;

        float easedProgress = EaseFunction.EaseCubicInOut.Ease(progress);

        float velocityCurve = (float)Math.Sin(easedProgress * Math.PI);
        float targetSpeed = velocityCurve * (float)MathHelper.PiOver2 * 1.25f;

        float glideStrength = 0.92f;
        float accelStrength = 0.112f;

        NPC.velocity.X = NPC.velocity.X * glideStrength + targetSpeed * dirToPlayer * accelStrength;

        float maxVel = 5f;
        if (Math.Abs(NPC.velocity.X) > maxVel)
            NPC.velocity.X = maxVel * Math.Sign(NPC.velocity.X);

        if (Math.Abs(NPC.velocity.X) > 0.1f)
            NPC.spriteDirection = -Math.Sign(NPC.velocity.X);

        if (Timer >= strollDuration)
        {
            StrollCount++;

            if (StrollCount >= targetStrolls)
            {
                float attackRoll = Main.rand.NextFloat();

                if (attackRoll < 0.3f) // 30% jump
                {
                    State = AIState.Jumping;
                    JumpPhase = 0;
                }
                else if (attackRoll < 0.65f) // 35% ground pound
                {
                    State = AIState.GroundPound;
                    JumpPhase = 0;
                    targetGroundPounds = 1;
                    groundPoundCount = 0;
                }
                else // 35% triple ground pound combo
                {
                    State = AIState.GroundPound;
                    JumpPhase = 0;
                    targetGroundPounds = 3;
                    groundPoundCount = 0;
                }
                StrollCount = 0;
            }
            Timer = 0;
        }
    }

    private void DoJumping(Player target)
    {
        if (NPC.velocity.Y == 0f)
        {
            NPC.velocity.X *= 0.5f;
            if (Math.Abs(NPC.velocity.X) < 0.1f)
                NPC.velocity.X = 0f;
            if (Timer >= 40f)
            {
                NPC.netUpdate = true;
                float dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);
                NPC.spriteDirection = -Math.Sign(dirToPlayer);
                float baseJumpHeight = -7.5f;
                float baseXVelocity = 2.15f;
                float overshootMod = Main.rand.NextFloat() < 0.33f ? 1.3f : 1f;
                NPC.velocity.Y = baseJumpHeight;
                NPC.velocity.X = baseXVelocity * dirToPlayer * overshootMod;
                squishScale = new Vector2(0.8f, 1.2f);
                JumpPhase = 1;
                Timer = 0;
            }
        }
        else
        {
            float dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);
            float maxSpeed = 4f;
            if ((dirToPlayer == 1 && NPC.velocity.X < maxSpeed) ||
                (dirToPlayer == -1 && NPC.velocity.X > -maxSpeed))
            {
                NPC.velocity.X += 0.15f * dirToPlayer;
            }

            if (Timer > 30 && NPC.velocity.Y > 0)
            {
                State = AIState.Strolling;
                JumpPhase = 0;
                Timer = 0;
                isWobbling = true;
                wobbleTimer = 0f;
            }
        }
    }

    private void DoGroundPound(Player target)
    {
        float dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);

        switch (JumpPhase)
        {
            case 0: // Charging phase
                NPC.velocity.X *= 0.5f;
                if (Math.Abs(NPC.velocity.X) < 0.1f)
                    NPC.velocity.X = 0f;

                float chargeTime;
                if (targetGroundPounds > 1 && groundPoundCount > 0)
                    chargeTime = 25f; // Very fast for combo follow-ups
                else if (Timer < 0)
                    chargeTime = 45f; // Normal follow-up
                else
                    chargeTime = 60f; // Initial attack

                if (Timer >= chargeTime)
                {
                    NPC.netUpdate = true;
                    NPC.spriteDirection = -Math.Sign(dirToPlayer);
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlamCharge") with { PitchVariance = 0.2f }, NPC.Center);

                    NPC.velocity.Y = -14f;
                    NPC.velocity.X = 2f * dirToPlayer;

                    for (int i = 0; i < 10; i++)
                    {
                        Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                            DustID.t_Slime, NPC.velocity.X * 0.4f, NPC.velocity.Y * 0.4f,
                            150, new Color(86, 162, 255, 100), 1.5f);
                        dust.noGravity = true;
                        dust.velocity *= 0.4f;
                    }

                    JumpPhase = 1;
                    Timer = 0;
                }
                break;

            case 1: // Airborne
                NPC.damage = 30;
                float maxSpeed = 3f;
                if ((dirToPlayer == 1 && NPC.velocity.X < maxSpeed) ||
                    (dirToPlayer == -1 && NPC.velocity.X > -maxSpeed))
                {
                    NPC.velocity.X += 0.1f * dirToPlayer;
                }

                if (Math.Abs(NPC.velocity.X) > 0.1f)
                    NPC.spriteDirection = -Math.Sign(NPC.velocity.X);

                if (NPC.velocity.Y > 0f)
                {
                    float currentXVel = NPC.velocity.X;
                    float targetDirection = target.Center.X > NPC.Center.X ? 1f : -1f;
                    float adjustedXVel = MathHelper.Lerp(currentXVel, targetDirection * 2f, 0.3f);

                    NPC.velocity = new Vector2(adjustedXVel, 12f);

                    NPC.spriteDirection = target.Center.X > NPC.Center.X ? -1 : 1;
                    JumpPhase = 2;
                    Timer = 0;
                }
                break;

            case 2: // Slamming down
                dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);
                if (Timer % 10 == 0)
                    NPC.spriteDirection = -Math.Sign(dirToPlayer);

                bool hitGround = false;
                int tileX = (int)(NPC.position.X / 16);
                int tileEndX = (int)((NPC.position.X + NPC.width) / 16);
                int tileY = (int)((NPC.position.Y + NPC.height) / 16);

                for (int i = tileX; i <= tileEndX; i++)
                {
                    Tile tile = Framing.GetTileSafely(i, tileY);
                    if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        hitGround = true;
                        break;
                    }
                }

                if (hitGround || NPC.velocity.Y == 0f)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlam") with { PitchVariance = 0.2f }, NPC.Center);
                    CameraSystem.shake = 6;
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 12)) * 8f;
                        Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y,
                            newColor: new Color(86, 162, 255, 100));
                        dust.noGravity = true;
                        dust.scale = 2f;
                    }

                    if (targetGroundPounds > 1)
                    {
                        Vector2 spawnPos = NPC.Center + new Vector2(0f, NPC.height / 2f);

                        int leftSlime = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X - 20, (int)spawnPos.Y, NPCID.BlueSlime);
                        if (leftSlime < Main.maxNPCs)
                        {
                            Main.npc[leftSlime].velocity = new Vector2(-4f + Main.rand.NextFloat(-1f, 0f),
                                                                       -3f + Main.rand.NextFloat(-1f, 1f));
                            Main.npc[leftSlime].scale = 1.2f;
                        }

                        int rightSlime = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X + 20, (int)spawnPos.Y, NPCID.GreenSlime);
                        if (rightSlime < Main.maxNPCs)
                        {
                            Main.npc[rightSlime].scale = 1.2f;
                        }

                        int centerSlime = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.SlimeSpiked);
                    }

                    JumpPhase = 3;
                    Timer = 0;

                    isWobbling = true;
                    wobbleTimer = 0f;
                }
                else
                {
                    float timeInPhase = Timer; // Time since entering slam phase
                    float accelCurve = MathHelper.Clamp(timeInPhase / 20f, 0.1f, 1f);
                    float baseAccel = 0.25f;
                    float currentAccel = baseAccel + (accelCurve * 0.4f);

                    NPC.velocity.Y += currentAccel;

                    if (NPC.velocity.Y > 14f)
                        NPC.velocity.Y = 14f;

                    if (Main.rand.NextBool(3))
                    {
                        Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                            DustID.t_Slime, NPC.velocity.X * 0.4f, NPC.velocity.Y * 0.4f,
                            150, new Color(86, 162, 255, 100), 1.5f);
                        dust.noGravity = true;
                        dust.velocity *= 0.4f;
                    }
                }
                break;

            case 3:
                NPC.damage = 12;
                NPC.velocity *= 0.8f;

                float recoveryTime = targetGroundPounds > 1 ? 45f : 90f;

                if (Timer >= recoveryTime)
                {
                    groundPoundCount++;

                    if (groundPoundCount >= targetGroundPounds)
                    {
                        State = AIState.Strolling;
                        JumpPhase = 0;
                        Timer = 0;
                        isWobbling = false;
                        groundPoundCount = 0;
                    }
                    else
                    {
                        JumpPhase = 0;
                        Timer = 0;
                        isWobbling = false;
                    }
                }
                break;
        }
    }

    public override void FindFrame(int frameHeight)
    {
        if (State == AIState.Jumping)
        {
            if (NPC.velocity.Y == 0f) // Charging jump
            {
                float animSpeed = 4f;
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.25f }, NPC.Center);
                        NPC.frame.Y = frameHeight;
                    }
                }
            }
            else // Airborne
            {
                NPC.frame.Y = frameHeight * 5;
            }
        }
        else if (State == AIState.GroundPound)
        {
            if (JumpPhase == 0) // Charging ground pound
            {
                float animSpeed = 2.5f;
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.25f }, NPC.Center);
                        NPC.frame.Y = frameHeight;
                    }
                }
            }
            else if (JumpPhase == 3) // Recovery wobble phase
            {
                float animSpeed = 3f; // Wobble animation speed
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    // Loop through frames 0-2 for wobble
                    if (NPC.frame.Y > frameHeight * 2)
                        NPC.frame.Y = 0;
                }
            }
            else // Airborne/slamming
            {
                NPC.frame.Y = frameHeight * 5;
            }
        }
        else // Strolling
        {
            float speed = Math.Abs(NPC.velocity.X);
            float animSpeed;
            bool isGliding = speed < 0.3f;

            if (isGliding)
            {
                if (!wasGliding)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}KingSlimeGlide") with { Pitch = -0.1f, Volume = 0.35f }, NPC.Center);
                }

                animSpeed = 60f;
                NPC.frame.Y = frameHeight * 4;
            }
            else if (speed < 1f)
            {
                if (NPC.frame.Y == frameHeight)
                    NPC.frame.Y = frameHeight;

                animSpeed = MathHelper.Lerp(12f, 6f, speed);

                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                        NPC.frame.Y = frameHeight;
                }
            }
            else
            {
                animSpeed = MathHelper.Lerp(6f, 3f, Math.Min(speed / 3f, 1f));

                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                        NPC.frame.Y = 0;
                }
            }

            wasGliding = isGliding;
        }
    }

    #region Helper Methods
    private void HandleRotation()
    {
        bool isAirborne = NPC.velocity.Y != 0f;
        if (isAirborne)
        {
            float fallSpeed = Math.Max(NPC.velocity.Y, 0f);
            float swayIntensity = fallSpeed * 0.005f;

            if (State == AIState.GroundPound && (JumpPhase == 1 || JumpPhase == 2))
            {
                float groundPoundIntensity = Math.Abs(NPC.velocity.Y) * 0.006f;
                swayIntensity = Math.Max(swayIntensity, groundPoundIntensity * 1.2f);
            }

            // Sinusoidal sway
            float swayFreq = 0.12f + Math.Abs(NPC.velocity.Y) * 0.009f;
            float sway = (float)Math.Sin(Timer * swayFreq) * swayIntensity * 2.25f;
            rotationSway = MathHelper.Lerp(rotationSway, sway, 0.08f);
        }
        else
        {
            rotationSway = MathHelper.Lerp(rotationSway, 0f, 0.15f);
        }
        NPC.rotation = rotationSway;
    }

    private void HandleSquishScale()
    {
        bool isAirborne = NPC.velocity.Y != 0f;
        if (isAirborne)
        {
            float yVel = NPC.velocity.Y;
            float stretchIntensity = Math.Abs(yVel) * 0.06f;

            if (State == AIState.GroundPound && JumpPhase == 2)
                stretchIntensity *= 0.75f;

            if (State == AIState.Jumping)
                stretchIntensity *= 0.55f;

            stretchIntensity = MathHelper.Clamp(stretchIntensity, 0f, 0.33f);
            if (yVel < 0) // Rising - stretch top
            {
                squishScale.Y = 1f + stretchIntensity;
                squishScale.X = 1f - stretchIntensity * 0.18f;
            }
            else // Falling - stretch bottom
            {
                squishScale.Y = 1f + stretchIntensity;
                squishScale.X = 1f - stretchIntensity * 0.25f;
            }
        }
        else if (isWobbling)
        {
            // Gelatin wobble effect after landing
            wobbleTimer += 0.15f;
            float wobbleIntensity = (State == AIState.GroundPound && JumpPhase == 3) ? 0.06f : 0.085f;

            float dampening = MathHelper.Clamp(1f - (wobbleTimer * 0.05f), 0.1f, 1f);
            wobbleIntensity *= dampening;
            float wobbleY = (float)Math.Sin(wobbleTimer * 1.2f) * wobbleIntensity;
            float wobbleX = (float)Math.Sin(wobbleTimer * 0.8f) * wobbleIntensity;
            squishScale.Y = 1f + wobbleY;
            squishScale.X = 1f + wobbleX;

            // Stop wobbling when intensity is low
            if (dampening <= 0.3f)
                isWobbling = false;
        }
        else
        {
            // Return to normal when grounded and not wobbling
            squishScale = Vector2.Lerp(squishScale, Vector2.One, 0.15f);
        }
        // Additional horizontal squish for movement
        if (Math.Abs(NPC.velocity.X) > 0.5f && !isAirborne)
        {
            float horizontalSquish = 0.995f; // Reduced from 0.98f
            squishScale.Y *= horizontalSquish;
            squishScale.X *= 1.001f; // Reduced from 1.003f
        }
    }

    private void HandleDustTrail()
    {
        if (Math.Abs(NPC.velocity.X) > 0.3f && Math.Abs(NPC.velocity.Y) < 2f)
        {
            int tileX = (int)(NPC.Center.X / 16f);
            int tileY = (int)((NPC.position.Y + NPC.height + 4) / 16f);

            if (WorldGen.InWorld(tileX, tileY))
            {
                Tile tile = Framing.GetTileSafely(tileX, tileY);

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    float dustChance = Math.Abs(NPC.velocity.X) * 0.4f;

                    if (Main.rand.NextFloat() < dustChance)
                    {
                        Vector2 dustPos = new Vector2(
                            NPC.position.X + Main.rand.Next(NPC.width),
                            NPC.position.Y + NPC.height - 8
                        );

                        Dust dust = Dust.NewDustDirect(dustPos, 8, 8, DustID.t_Slime);
                        dust.velocity.X = NPC.velocity.X * 0.3f + Main.rand.NextFloat(-1f, 1f);
                        dust.velocity.Y = Main.rand.NextFloat(-1f, 0.5f);
                        dust.color = new Color(86, 162, 255, 100);
                        dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                        dust.alpha = Main.rand.Next(50, 150);
                    }
                }
            }
        }
    }
    #endregion

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Texture2D capeTexture = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}NPCs/Bosses/KingSlime/KingSlime_Cape").Value;
        Vector2 origin = NPC.frame.Size() / 2f;

        float groundOffset = NPC.frame.Height * (1f - squishScale.Y) / 2f;
        Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, groundOffset + NPC.gfxOffY);

        SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(drawColor * 0.75f), NPC.rotation, origin, NPC.scale * squishScale, effects, 0f);
        spriteBatch.Draw(capeTexture, drawPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale * squishScale, effects, 0f);

        return false;
    }
}