using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reverie.Common.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items
{
    public class SeedBag : ModItem
    {
        public override string Texture => Assets.Terraria.Items.Weapons + Name;

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.MagicSummonHybrid;
            Item.damage = 3;
            Item.knockBack = 0.8f;
            Item.width = 24;
            Item.height = 32;
            Item.useTime = Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.RaiseLamp;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.mana = 8;
            Item.shootSpeed = 14f;
            Item.holdStyle = ItemHoldStyleID.HoldFront;
            Item.shoot = ModContent.ProjectileType<SeedProj>();
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            float armRotation = MathHelper.ToRadians(80f);
            if (player.direction == 1)
            {
                player.itemLocation.X -= heldItemFrame.X + 14;
                player.itemLocation.Y += heldItemFrame.Y + 18;
                armRotation = -armRotation;
            }
            if (player.direction != 1)
            {
                player.itemLocation.X -= heldItemFrame.X - 14;
                player.itemLocation.Y += heldItemFrame.Y + 18;
            }
            Item.scale = 0.85f;
            Item.autoReuse = false;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);           
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            float armRotation = MathHelper.ToRadians(80f);
            if (player.direction == 1)
            {
                player.itemLocation.X -= heldItemFrame.X + 4;
                player.itemLocation.Y += heldItemFrame.Y + 8;
                armRotation = -armRotation;
            }
            if (player.direction != 1)
            {
                player.itemLocation.X -= heldItemFrame.X - 4;
                player.itemLocation.Y += heldItemFrame.Y + 8;
            }
            Item.scale = 0.85f;
            Item.autoReuse = false;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int numProjectiles = 3;
            float spreadAngle = MathHelper.ToRadians(8f);

            for (int i = 0; i < numProjectiles; i++)
            {
                Vector2 newVelocity = velocity.RotatedBy(MathHelper.Lerp(-spreadAngle / 2, spreadAngle / 2, i / (float)(numProjectiles - 1)));

                newVelocity *= 1f - Main.rand.NextFloat(0.2f);

                Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);
            }

            return false;
        }
    }

    public class SeedProj : StickyProjectile
    {
        public override string Texture => Assets.Terraria.Projectiles.Dir + Name;
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
            float scaleFactor = 0.4f; // Adjust this value as needed
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
            Vector2 spawnPosition = GetOffScreenSpawnPosition();
            Vector2 direction = Projectile.Center - spawnPosition;
            direction.Normalize();
            Vector2 velocity = direction * Main.rand.NextFloat(3f, 5f);

            int birdProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity,
                ModContent.ProjectileType<BirdProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            if (birdProj != Main.maxProjectiles)
            {
                Main.projectile[birdProj].ai[0] = Projectile.whoAmI; // Store the seed's index
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
            // Create larger dust particles
            for (int i = 0; i < 2; i++)
            {
                Dust dust = CreateDust(1.1f, scaleFactor);
                dust.position = (dust.position + Projectile.Center) / 2f;
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Create smaller dust particle
            Dust smallDust = CreateDust(0.6f, scaleFactor);
            smallDust.position = Vector2.Lerp(smallDust.position, Projectile.Center, 5f / 6f);
            smallDust.velocity *= 0.1f;
            smallDust.noGravity = true;
            smallDust.fadeIn = 0.9f * scaleFactor;
        }

        private Dust CreateDust(float scale, float scaleFactor)
        {
            int dustIndex = Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.Hay,  // Assuming 4 is the ID for orange-colored dust
                Projectile.velocity.X,
                Projectile.velocity.Y,
                50,
                Color.PaleGoldenrod,
                scale
            );

            Dust dust = Main.dust[dustIndex];
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

        public override string Texture => Assets.Terraria.Projectiles.Dir + Name;

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
                SoundEngine.PlaySound(new SoundStyle($"{Assets.SFX.Directory}Bird_" + Main.rand.Next(1, 2))
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
                int seedIndex = (int)Projectile.ai[0];
                Projectile seed = Main.projectile[seedIndex];

                if (seed.active && seed.type == ModContent.ProjectileType<SeedProj>())
                {
                    Vector2 toSeed = seed.Center - Projectile.Center;
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

            // Update rotation based on velocity
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
            }

            // Flip the sprite horizontally based on movement direction
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            textureIndex = Main.rand.Next(3);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = textureIndex switch
            {
                0 => ModContent.Request<Texture2D>($"{Assets.Terraria.Projectiles.Dir}BirdProj").Value,
                1 => ModContent.Request<Texture2D>($"{Assets.Terraria.Projectiles.Dir}BirdProj2").Value,
                _ => ModContent.Request<Texture2D>($"{Assets.Terraria.Projectiles.Dir}BirdProj3").Value
            };

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
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
