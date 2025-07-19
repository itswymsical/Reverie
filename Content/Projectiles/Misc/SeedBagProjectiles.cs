using Reverie.Core.Projectiles.Actors;
using Terraria.Audio;
using Terraria.DataStructures;

using static Reverie.Reverie;

namespace Reverie.Content.Projectiles.Misc
{
    public class SeedProj : StickyProjectileActor
    {
        public override int MaxStickies => 7;

        private int birdSpawnTimer = 0;
        public override void SetDefaults()
        {
            Projectile.damage = 1;
            Projectile.DamageType = DamageClass.MagicSummonHybrid;
            Projectile.width = Projectile.height = 4;

            stickToTile = true;
            stickToNPC = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.localNPCHitCooldown = 300;
            Projectile.timeLeft = 260;
            Projectile.penetrate = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.minion = true;
            base.SetDefaults();
        }

        public override void AI()
        {
            if (!stickingToNPC && !stickingToTile)
            {
                Projectile.velocity.Y += 0.2f;
                if (Projectile.velocity.Y > 10)
                    Projectile.velocity.Y = 10;

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            base.AI();
            var scaleFactor = 0.4f;
            CreateDustEffects(scaleFactor);

            birdSpawnTimer++;
            if (birdSpawnTimer >= 30)
            {
                birdSpawnTimer = 0;
                if (Main.rand.NextBool(4))
                    SpawnBird();
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
        }

        private void SpawnBird()
        {
            var spawnPosition = GetOffScreenSpawnPosition();
            var direction = Projectile.Center - spawnPosition;
            direction.Normalize();
            var velocity = direction * Main.rand.NextFloat(3f, 5f);

            var birdProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity,
                ModContent.ProjectileType<BirdProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            if (birdProj != Main.maxProjectiles)
            {
                Main.projectile[birdProj].ai[0] = Projectile.whoAmI;
            }
        }

        private Vector2 GetOffScreenSpawnPosition()
        {
            Vector2 spawnPos;
            do
            {
                spawnPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * 1000f;
            } while (spawnPos.X > 0 && spawnPos.X < Main.screenWidth && spawnPos.Y > 0 && spawnPos.Y < Main.screenHeight);

            return spawnPos;
        }

        public override bool MinionContactDamage() => false;

        private void CreateDustEffects(float scaleFactor)
        {
            for (var i = 0; i < 2; i++)
            {
                var dust = CreateDust(1.1f, scaleFactor);
                dust.position = (dust.position + Projectile.Center) / 2f;
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            var smallDust = CreateDust(0.6f, scaleFactor);
            smallDust.position = Vector2.Lerp(smallDust.position, Projectile.Center, 5f / 6f);
            smallDust.velocity *= 0.1f;
            smallDust.noGravity = true;
            smallDust.fadeIn = 0.9f * scaleFactor;
        }

        private Dust CreateDust(float scale, float scaleFactor)
        {
            var dustIndex = Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.Hay,
                Projectile.velocity.X,
                Projectile.velocity.Y,
                50,
                Color.PaleGoldenrod,
                scale
            );

            var dust = Main.dust[dustIndex];
            dust.scale *= scaleFactor;
            return dust;
        }
    }

    public class BirdProj : ModProjectile
    {
        private const float APPROACH_SPEED = 6f;
        private const int ATTACK_TIME = 300;
        private const int RETREAT_TIME = 120;
        private int soundTimer = 0;
        private int textureIndex = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 0;
            Main.projFrames[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.damage = 2;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.width = 18;
            Projectile.height = 16;

            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.localNPCHitCooldown = 90;

            Projectile.timeLeft = ATTACK_TIME + RETREAT_TIME;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 2;
            Projectile.friendly = true;
            Projectile.minion = true;
            base.SetDefaults();
        }
        public override bool? CanCutTiles() => true;

        public override bool MinionContactDamage() => true;

        public override void AI()
        {

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            soundTimer++;
            if (soundTimer >= 120)
            {
                soundTimer = 0;
                SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}Bird_" + Main.rand.Next(1, 2))
                {
                    MaxInstances = 7,
                    Volume = 0.1f,
                    Pitch = 0.3f,
                    PitchVariance = 1.2f,
                    PlayOnlyIfFocused = true,
                }, Projectile.position);
            }

            if (Projectile.timeLeft > RETREAT_TIME)
            {
                var seedIndex = (int)Projectile.ai[0];
                var seed = Main.projectile[seedIndex];

                if (seed.active && seed.type == ModContent.ProjectileType<SeedProj>())
                {
                    var toSeed = seed.Center - Projectile.Center;
                    if (toSeed.Length() > 5f)
                    {
                        toSeed.Normalize();
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toSeed * APPROACH_SPEED, 0.1f);
                    }
                    else
                    {
                        Projectile.velocity *= 0.95f;
                    }
                }
                else
                {
                    Projectile.timeLeft = RETREAT_TIME;
                }
            }
            else
            {
                Projectile.velocity.Y -= 0.1f;
                Projectile.velocity.X *= 0.99f;
            }

            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            textureIndex = Main.rand.Next(3);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = textureIndex switch
            {
                0 => ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Projectiles/Misc/BirdProj").Value,
                1 => ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Projectiles/Misc/BirdProj2").Value,
                _ => ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Projectiles/Misc/BirdProj3").Value
            };

            var frameHeight = texture.Height / Main.projFrames[Projectile.type];
            var sourceRectangle = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

            var drawOrigin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);
            for (var k = 0; k < Projectile.oldPos.Length; k++)
            {
                var drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                var color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, sourceRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                sourceRectangle,
                lightColor,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale,
                SpriteEffects.None,
                0f);

            return false;
        }
    }
}
