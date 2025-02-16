using Reverie.Core.Missions;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;

namespace Reverie.Common.NPCs
{

    public enum SlimeState
    {
        Idle,
        PrepareSlamAttack,
        SlamAttack,
    }
    public class SlimeGlobal : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCAIStyleID.Slime;

        private Vector2 squishScale = Vector2.One;
        private readonly float rotation;
        private float stateTimer;
        private int consecutiveSlams;

        private const int MAX_CONSECUTIVE_SLAMS = 3;
        private const float SLAM_SPEED = 7f;
        private SlimeState currentState = SlimeState.Idle;

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type != NPCAIStyleID.Slime) return;

            if (NPC.AnyNPCs(NPCID.KingSlime))
            {
                npcLoot.Add(ItemDropRule.DropNothing());
            }
        }
        public override void SetDefaults(NPC npc)
        {
            if (npc.type != NPCAIStyleID.Slime) return;

            npc.HitSound = new SoundStyle($"{SFX_DIRECTORY}SlimeHit") with { Volume = 0.46f, PitchVariance = 0.3f, MaxInstances = 8 };
            npc.DeathSound = new SoundStyle($"{SFX_DIRECTORY}SlimeKilled") with { Volume = 0.76f, PitchVariance = 0.2f, MaxInstances = 8 };
        }

        public override void AI(NPC npc)
        {
            if (npc.type != NPCAIStyleID.Slime) return;
            var mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            var AFallingStar = mPlayer.GetMission(MissionID.A_FALLING_STAR);

            if (AFallingStar?.Progress == MissionProgress.Active)
            {
                var currentSet = AFallingStar.ObjectiveIndex[AFallingStar.CurObjectiveIndex];
                if (AFallingStar.CurObjectiveIndex >= 3)
                {
                    UpdateState(npc);
                }          
            }
            else if (npc.life < npc.lifeMax * .44f)
            {
                UpdateState(npc, postFallingStar: true);
            }

            HandleSquishScale(npc);
        }

        private void UpdateState(NPC npc, bool postFallingStar = false)
        {
            stateTimer++;
            switch (currentState)
            {

                case SlimeState.Idle:
                    if (!postFallingStar)
                    {
                        if (stateTimer > 540f)
                        {
                            npc.aiStyle = -1;
                            currentState = SlimeState.PrepareSlamAttack;
                            HandleSlamSetup(npc);
                            stateTimer = 0;
                        }
                    }
                    else
                    {
                        npc.aiStyle = -1;
                        currentState = SlimeState.PrepareSlamAttack;
                        HandleSlamSetup(npc);
                    }
                    break;

                case SlimeState.PrepareSlamAttack:
                    if (HandleSlamSetup(npc))
                    {
                        HandleSlam(npc);
                    }
                    break;

                case SlimeState.SlamAttack:
                    if (HandleSlam(npc))
                    {
                        consecutiveSlams++;
                        if (consecutiveSlams >= MAX_CONSECUTIVE_SLAMS)
                        {
                            npc.aiStyle = NPCAIStyleID.Slime;
                            currentState = SlimeState.Idle;
                        }
                        else
                            HandleSlamSetup(npc);
                    }
                    break;
            }
        }

        private bool HandleSlamSetup(NPC npc)
        {

            if (npc.velocity.Y == 0f)
            {
                if (!npc.localAI[1].Equals(1f))
                {
                    npc.localAI[1] = 1f;
                }

                npc.direction = npc.spriteDirection = npc.Center.X < Main.player[npc.target].Center.X ? 1 : -1;

                float yVel = npc.velocity.Y = -6.74f;
                float xVel = npc.velocity.X = .84f * npc.direction;

                if (Main.masterMode)
                {
                    xVel = xVel * -1.77f;
                    yVel = yVel * -1.97f;
                }

                else if (Main.expertMode)
                {
                    xVel = xVel * -1.77f;
                    yVel = yVel * -1.47f;
                }

                for (int i = 0; i < 10; i++)
                {
                    int dust = Dust.NewDust(npc.position, npc.width, npc.height,
                        DustID.t_Slime, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f,
                        150, npc.color, 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 0.4f;
                }
            }
            else // In air
            {
                npc.localAI[1] = 0f;
                float maxSpeed = 3f;
                if (Main.masterMode)
                {
                    maxSpeed = 6f;
                }
                else if (Main.expertMode)
                {
                    maxSpeed = 4.11f;

                }

                if ((npc.direction == 1 && npc.velocity.X < maxSpeed) ||
                    (npc.direction == -1 && npc.velocity.X > -maxSpeed))
                {
                    npc.velocity.X += 0.1f * npc.direction;
                }
                if (npc.velocity.Y > 0f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HandleSlam(NPC npc)
        {

            if (stateTimer == 0)
            {
                Player target = Main.player[npc.target];
                Vector2 toTarget = target.Center - npc.Center;
                float targetAngle = toTarget.ToRotation();
                npc.velocity = new Vector2(
                    target.Center.X > npc.Center.X ? 2f : -2f,
                    SLAM_SPEED
                );
            }

            bool hitGround = false;
            int tileX = (int)(npc.position.X / 16);
            int tileEndX = (int)((npc.position.X + npc.width) / 16);
            int tileY = (int)((npc.position.Y + npc.height) / 16);

            for (int i = tileX; i <= tileEndX; i++)
            {
                Tile tile = Framing.GetTileSafely(i, tileY);
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    hitGround = true;
                    break;
                }
            }

            if (hitGround || npc.velocity.Y == 0f)
            {
                if (!npc.localAI[0].Equals(1f))
                {
                    npc.localAI[0] = 1f;
                }

                stateTimer++;
                npc.velocity *= 0.8f;

                if (stateTimer > 10f)
                {
                    npc.noTileCollide = false;
                    npc.noGravity = false;
                    npc.localAI[0] = 0f;
                    return true;
                }
            }
            else
            {
                npc.velocity.Y += 0.5f;
                if (npc.velocity.Y > SLAM_SPEED)
                    npc.velocity.Y = SLAM_SPEED;

            }

            return false;
        }

        private void HandleSquishScale(NPC npc)
        {
            const float MAX_STRETCH = 1.5f;
            const float MIN_SQUISH = 0.7f;

            float yVelocityFactor = MathHelper.Clamp(npc.velocity.Y / 16f, -1f, 1f);

            if (npc.velocity.Y != 0)
            {
                if (npc.velocity.Y < 0)
                {
                    squishScale.Y = MathHelper.Lerp(1f, MAX_STRETCH, -yVelocityFactor);
                    squishScale.X = MathHelper.Lerp(1f, MIN_SQUISH, -yVelocityFactor);
                }
                else
                {
                    squishScale.Y = MathHelper.Lerp(1f, MIN_SQUISH, yVelocityFactor);
                    squishScale.X = MathHelper.Lerp(1f, MAX_STRETCH, yVelocityFactor);
                }
            }
            else
            {
                squishScale = Vector2.Lerp(squishScale, Vector2.One, 0.15f);
            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type != NPCAIStyleID.Slime) return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

            Texture2D texture = TextureAssets.Npc[npc.type].Value;

            int frameHeight = texture.Height / 2;
            int frameWidth = texture.Width;

            int frameY = (int)npc.frameCounter / 8 % 2 * frameHeight;

            Rectangle sourceRectangle = new(0, frameY, frameWidth, frameHeight);

            Color finalColor = npc.color;
            finalColor.R = (byte)((finalColor.R * drawColor.R) / 255);
            finalColor.G = (byte)((finalColor.G * drawColor.G) / 255);
            finalColor.B = (byte)((finalColor.B * drawColor.B) / 255);
            finalColor.A = npc.color.A;

            Vector2 drawPos = npc.Center - Main.screenPosition;
            SpriteEffects spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(
                texture,
                drawPos,
                sourceRectangle,
                finalColor,
                rotation,
                new Vector2(frameWidth / 2, frameHeight / 2),
                squishScale * npc.scale,
                spriteEffects
            );

            return false;
        }
    }
}