using Reverie.Core.Graphics;
using Reverie.Core.Interfaces;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Effects;

namespace Reverie.Content.NPCs.Warden
{
    [AutoloadBossHead]
	public class WardenArm : ModNPC, IDrawPrimitive
	{
        public enum AIState
        {
            Idle,
            Grabbing,
            Dashing,
            Laser,
            Deterred,
            Grabbed
        }
        private enum ArmType
        {
            Left,
            Right
        }
        private AIState State
        {
            get => (AIState)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        public float AITimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        private ArmType Arm
        {
            get => (ArmType)NPC.localAI[0];
            set => NPC.localAI[0] = (float)value;
        }

        private bool flagged;
        private int dashCount;
        private int laserShots;

        private int ShotTimer;
        private int DashTimer;

        private int hitsTaken;

        private Player grabbedPlayer;
        
        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 4;

            Main.npcFrameCount[NPC.type] = 3;
        }
        public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 62;
			NPC.height = 96;

			NPC.damage = 18;
			NPC.defense = 12;
			NPC.lifeMax = 520;

			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.lavaImmune = true;
			NPC.netUpdate = true;

			NPC.knockBackResist = 0f;
            NPC.HitSound = new SoundStyle($"{SFX_DIRECTORY}WoodHit2")
            {
                Volume = 1.4f,
                PitchVariance = 0.2f,
                MaxInstances = 3,
            };
            NPC.DeathSound = SoundID.Item14;
		}
        public override void BossHeadRotation(ref float rotation) => rotation = NPC.rotation;

        private void Move(Vector2 position, float speed)
        {
            var direction = NPC.DirectionTo(position);

            var velocity = direction * speed;

            NPC.velocity = Vector2.SmoothStep(NPC.velocity, velocity, 0.2f);
        }

        public override bool CheckActive()
        {
            if (!NPC.AnyNPCs(ModContent.NPCType<WoodenWarden>()))
                NPC.active = false;

            return base.CheckActive();
        }

        public override void FindFrame(int frameHeight)
		{
            NPC.spriteDirection = Arm == ArmType.Left ? -1 : 1;
            
			if (State == AIState.Laser)
			{
				NPC.frame.Y = 1 * frameHeight;
			}
			else if (State == AIState.Grabbing)
			{
				NPC.frame.Y = 0 * frameHeight;
			}
            else
            {
                NPC.frame.Y = 2 * frameHeight;
            }
        }

        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;
        private Color color = new(108, 187, 86);
        private readonly Vector2 Size = new(20, 20);
        public override void AI()
        {
            ManageCaches();
            ManageTrail();
            var boss = Main.npc[(int)NPC.ai[3]];
            var target = Main.player[NPC.target];
            NPC.TargetClosest(true); 
            switch (State)
            {
                case AIState.Idle:
                    IdleHover();
                    break;
                case AIState.Grabbing:
                    Grabbing();
                    break;
                case AIState.Dashing:
                    Dashing();
                    break;
                case AIState.Laser:
                    PelletAttack(target);
                    break;
                case AIState.Deterred:
                    Deterred();
                    break;
                case AIState.Grabbed:
                    CrushPlayer(target);
                    break;
            }

            AITimer++;
            if (AITimer >= 300) // Adjust timing as needed
            {
                if (State == AIState.Idle)
                {
                    if (Arm == ArmType.Left)
                    {
                        if (Main.rand.NextBool(2))
                            State = AIState.Laser;

                        else
                            State = AIState.Grabbing;
                    }
                    else if (Arm == ArmType.Right)
                    {
                        if (Main.rand.NextBool(2))
                            State = AIState.Laser;
                        
                        else
                            State = AIState.Dashing;                       
                    }
                }
                else if (State == AIState.Grabbing || State == AIState.Laser || State == AIState.Dashing || State == AIState.Grabbed)
                {
                    State = AIState.Idle;
                }
                AITimer = 0;
            }
        }

        private void ManageCaches()
        {
            var player = Main.LocalPlayer;
            var pos = NPC.Center + player.DirectionTo(NPC.Center) * (Size.Length() * Main.rand.NextFloat(0.5f, 1.1f)) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.0f, 4.0f);

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
            var pos = NPC.Center + player.DirectionTo(NPC.Center) * (Size.Length() * Main.rand.NextFloat(0.5f, 1.1f)) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.0f, 4.0f);

            trail ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(5), factor => factor * 16, factor =>
            {
                if (factor.X >= 1.98f)
                    return Color.White * 0;
                return new Color(color.R, color.G, color.B) * 0.4f * (float)Math.Pow(factor.X, 2) * (float)Math.Sin(NPC.timeLeft / 150f * 4);
            });
            trail.Positions = [.. cache];

            trail2 ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(5), factor => factor * 16, factor =>
            {
                if (factor.X >= 1.98f)
                    return Color.White * 0;
                return new Color(color.R, color.G, color.B) * 0.4f * (float)Math.Pow(factor.X, 2) * (float)Math.Sin(NPC.timeLeft / 150f * 4);
            });
            trail2.Positions = [.. cache];

            trail.NextPosition = pos + NPC.velocity;
            trail2.NextPosition = pos + NPC.velocity;
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

                    effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.03f);
                    effect.Parameters["repeats"]?.SetValue(8f);
                    effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
                    effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value);
                    effect.Parameters["sampleTexture2"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Bloom").Value);

                    trail?.Render(effect);

                    effect.Parameters["sampleTexture2"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value);

                    trail2?.Render(effect);
                }
            }
        }

        private void IdleHover()
        {
            var target = Main.player[NPC.target];
            var angle = target.Center - NPC.Center;
            NPC.rotation = angle.ToRotation() + MathHelper.PiOver2;
            angle.Normalize();
            angle.X *= 3f;
            angle.Y *= 3f;

            var boss = Main.npc[(int)NPC.ai[3]];

            var position = boss.Center - new Vector2(172, -60);
            if (Arm == ArmType.Right)
            {
                position = boss.Center - new Vector2(-172, -60);
            }
            var speed = Vector2.Distance(NPC.Center, position);
            speed = MathHelper.Clamp(speed, -14f, 14f);
            Move(position, speed);
        }

        private void Grabbing()
        {
            NPC.damage = 0;
            var target = Main.player[NPC.target];
            var angle = target.Center - NPC.Center;
            var point = new Vector2(target.Center.X, target.Center.Y);
            NPC.rotation = angle.ToRotation() + MathHelper.PiOver2;
            angle.Normalize();
            angle.X *= 3f;
            angle.Y *= 3f;

            var speed = Vector2.Distance(NPC.Center, point);
            speed = MathHelper.Clamp(speed, -10f, 10f);
            Move(point, speed);

            if (AITimer >= 90)
            {
                NPC.knockBackResist = 0f;
                State = AIState.Deterred;
                ShakeEffect(5);
                AITimer = 0;
            }
            if (NPC.Hitbox.Intersects(target.Hitbox))
            {
                State = AIState.Grabbed;
                AITimer = 0;
            }
        }

        private void Deterred()
        {
            NPC.position += new Vector2(Main.rand.Next(-3, 4), Main.rand.Next(-3, 4)); // Shake effect
            NPC.velocity.Y = 0f;
            NPC.velocity.X = 0;
            var boss = Main.npc[(int)NPC.ai[3]];

            if (AITimer >= 180) // 3 seconds
            {
                SoundEngine.PlaySound(SoundID.Item32, NPC.position);
                State = AIState.Idle;
                AITimer = 0;
            }
        }

        public static string CrushQuotes(Player target)
        {
            var gender = "";
            if (target.Male)
                gender = "his";
            
            else if (!target.Male)
                gender = "her";
            
            else
                gender = "their";
            

            var OverloadQuotes = new string[]
            {
                " was crushed into a pool of blood.",
                " was turned into a ball of bramble.",
                " was brutally squeezed to death.",
                " couldn't break free.",
                " couldn't wiggle " + gender + " way out this one."
            };
            
            var randomQuote = Main.rand.Next(OverloadQuotes.Length);

            return OverloadQuotes[randomQuote];
        }

        private void CrushPlayer(Player target)
        {
            const float RaiseSpeed = 2f; // Adjust this value to control how fast the player is raised
            const float MaxRaiseHeight = 200f; // Maximum height to raise the player
            const float HorizontalHoldStrength = 0.9f; // How strongly to restrict horizontal movement (0-1)

            // Store the initial grab position if it's not set
            if (grabInitialPosition == Vector2.Zero)
            {
                grabInitialPosition = target.position;
            }

            // Calculate the desired position (raising the player)
            var desiredPosition = grabInitialPosition;
            desiredPosition.Y -= Math.Min(AITimer * RaiseSpeed, MaxRaiseHeight);

            // Move the player towards the desired position
            target.position = Vector2.Lerp(target.position, desiredPosition, 0.2f);

            // Restrict horizontal movement
            target.velocity.X *= 1f - HorizontalHoldStrength;

            // Disable player's ability to jump
            target.jump = 0;

            // Update NPC position to match player
            NPC.Center = target.Center;

            // Handle hits taken
            if (NPC.justHit)
            {
                hitsTaken++;
                CombatText.NewText(NPC.Hitbox, Color.Red, $"Hits: {hitsTaken}!", true);
            }

            // Check for release conditions
            var hitsRequired = Main.masterMode ? 5 : 3;
            if (hitsTaken >= hitsRequired)
            {
                State = AIState.Deterred;
                hitsTaken = 0;
                ResetGrab();
            }

            PunchCameraModifier modifier = new(NPC.Center,
                (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(),
                Math.Abs(AITimer / 48f), 6f, 20, 1000f, FullName);

            if (AITimer > 240)
            {
                var damage = Main.masterMode ? 150 : 100;
                target.Hurt(PlayerDeathReason.ByCustomReason(target.name + CrushQuotes(target)), damage, 0);

                for (var num = 0; num < 12; num++)
                {
                    Dust.NewDust(target.Center, NPC.width, NPC.height, DustID.Blood, newColor: Color.DarkRed, Scale: 2f);
                }
                SoundEngine.PlaySound(SoundID.Item14, NPC.position);

                AITimer = 0;
                State = AIState.Idle;
                ResetGrab();
            }

            AITimer++;
        }

        private Vector2 grabInitialPosition;

        private void ResetGrab()
        {
            grabInitialPosition = Vector2.Zero;
            hitsTaken = 0;
        }

        private void ShakeEffect(float value)
        {
            var shakeOffset = new Vector2(Main.rand.NextFloat(-0.5f, 1f), Main.rand.NextFloat(-0.5f, 1f)) * value;
            NPC.position += shakeOffset;
        }

        private bool hasDashed = false;

        private void Dashing()
        {
            var target = Main.player[NPC.target];
            var angle = target.Center - NPC.Center;
            var point = new Vector2(target.Center.X, target.Center.Y);
            NPC.rotation = angle.ToRotation() + MathHelper.PiOver2;
            angle.Normalize();
            angle.X *= 3f;
            angle.Y *= 3f;

            var speed = Vector2.Distance(NPC.Center, point);
            speed = MathHelper.Clamp(speed, -7f, 7f);
            Move(point, speed);
            if (AITimer >= 90)
            {
                NPC.knockBackResist = 0f;
                State = AIState.Idle;
                AITimer = 0;
            }
        }

        private void PelletAttack(Player target)
        {
            ShotTimer++;
            var angle = target.Center - NPC.Center;
            NPC.rotation = angle.ToRotation() + MathHelper.PiOver2;
            angle.Normalize();

            var position = target.Center - new Vector2(Arm == ArmType.Right ? 240 : -240, 90);
            var speed = MathHelper.Clamp(Vector2.Distance(NPC.Center, position), -14f, 14f);
            Move(position, speed);

            var shotDelay = NPC.life < NPC.lifeMax * 0.20f ? 60 : 90;

            if (ShotTimer >= shotDelay)
            {
                var spreadAngle = MathHelper.ToRadians(30);
                for (var i = -1; i <= 1; i++)
                {
                    var velocity = angle.RotatedBy(spreadAngle * i) * 9.5f;
                    Projectile.NewProjectile(default, NPC.Center, velocity, ProjectileID.JungleSpike, 5, 0f, Main.myPlayer);
                }

                // Recoil effect
                NPC.velocity -= angle * 9f; // Adjust the multiplier for stronger/weaker recoil

                ShotTimer = 0;
            }

            // Gradually reduce recoil
            NPC.velocity *= 0.95f;
        }
    }
}
