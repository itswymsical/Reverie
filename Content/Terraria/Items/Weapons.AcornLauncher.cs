using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reverie.Content.Sylvanwalde.Tiles.Canopy;
using Reverie.Content.Terraria.Tiles.Canopy;
using Reverie.Helpers;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items
{
    public class AcornLauncher : ModItem
    {
        public override string Texture => Assets.Terraria.Items.Weapons + Name;
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 17;

            Item.width = 92;
            Item.height = 34;

            Item.useTime = 20;
            Item.useAnimation = 20;

            Item.useAmmo = ItemID.Acorn;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = new SoundStyle($"{Assets.SFX.Directory}AcornCharge")
            {
                Volume = 1f,
                PitchVariance = 0.2f,
                MaxInstances = 2,
            };
            Item.rare = ItemRarityID.Blue;

            Item.autoReuse = false;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shootSpeed = 7f;

            Item.shoot = ModContent.ProjectileType<AcornLauncherProj>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 25f;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
            Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<AcornLauncherProj>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            // This allows the weapon to turn the player
            return player.ownedProjectileCounts[ModContent.ProjectileType<AcornLauncherProj>()] <= 0;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(RecipeGroupID.Wood, 16);
            recipe.AddIngredient(ModContent.ItemType<Alluvium>(), 7);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class AcornLauncherProj : ModProjectile
    {
        public override string Texture => Assets.Terraria.Items.Weapons + "AcornLauncher";
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

        private const float ChargeTime = 33f;
        private const float FireRate = 18f;

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
        }
        public override bool? CanDamage() => false;
        
        public override bool PreAI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead || !owner.channel)
            {
                Projectile.Kill();
                return false;
            }

            Vector2 aimDirection = Vector2.Normalize(Main.MouseWorld - owner.Center);
            Projectile.Center = owner.Center;
            if (Main.myPlayer == Projectile.owner)
            {
                // Only change direction on the controlling player's side
                Projectile.direction = (Main.MouseWorld.X > owner.Center.X) ? 1 : -1;
                Projectile.netUpdate = true;
            }

            owner.ChangeDir(Projectile.direction);
            Projectile.spriteDirection = Projectile.direction;

            Projectile.rotation = aimDirection.ToRotation(); // + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0)

            if (Main.myPlayer == Projectile.owner)
            {
                if (State == AIState.Charging)
                {
                    Projectile.ai[1]++;
                    if (Projectile.ai[1] >= ChargeTime)
                    {
                        State = AIState.Firing;                       
                        Projectile.ai[1] = 0;
                    }
                }
                else if (State == AIState.Firing)
                {
                    Projectile.ai[1]++;
                    if (Projectile.ai[1] >= FireRate && owner.HasAmmo(owner.HeldItem))
                    {
                        FireProjectile(owner, aimDirection);
                        Projectile.ai[1] = 0;
                        State = AIState.Charging;
                    }
                }
            }

            // Update player animation
            SetOwnerAnimation(owner);

            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new Vector2(texture.Width * 0.3f, texture.Height * 0.55f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            // No need for sprite effects or rotation adjustments
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            // Draw charge progress bar
            if (State == AIState.Charging)
            {
                float chargeProgress = Projectile.ai[1] / ChargeTime;
                Vector2 barOffset = new Vector2(0, 30).RotatedBy(Projectile.rotation);
                Vector2 barPosition = drawPos + barOffset;
                Rectangle barRect = new Rectangle((int)barPosition.X - 15, (int)barPosition.Y, 30, 5);
                Color barColor = Color.Lerp(Color.Red, Color.Green, chargeProgress);

                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, barRect, Color.White * 0.5f);
                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(barRect.X, barRect.Y, (int)(barRect.Width * chargeProgress), barRect.Height), barColor);
            }

            return false;
        }
        private void FireProjectile(Player owner, Vector2 direction)
        {
            owner.ConsumeItem(ItemID.Acorn);
            int type = ModContent.ProjectileType<AcornProj>();
            float speed = owner.HeldItem.shootSpeed;
            int damage = owner.HeldItem.damage;
            float knockback = owner.HeldItem.knockBack;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Center, direction * speed, type, damage, knockback, owner.whoAmI);
            
            // Play shoot sound
            SoundEngine.PlaySound(new SoundStyle($"{Assets.SFX.Directory}AcornFire")
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

            // Set the player's arm position to resemble gun holding animation
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (owner.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2);
        }
    }

    public class AcornProj : ModProjectile
    {
        public override string Texture => Assets.Terraria.Projectiles.Dir + Name;
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
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Helper.SpawnDustCloud(Projectile.position, Projectile.width, Projectile.height, DustID.t_LivingWood);
            for (int i = 0; i < 3; ++i)
            {
                Projectile.NewProjectile(default, Projectile.Center, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * 4f,
                    ModContent.ProjectileType<AcornShrapnel>(), (int)(Projectile.damage * 0.5f), 0.5f, Projectile.owner);
            }
        }
    }

    public class AcornShrapnel : ModProjectile
    {
        public override string Texture => Assets.Terraria.Projectiles.Dir + Name;
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
            Helper.SpawnDustCloud(Projectile.position, Projectile.width, Projectile.height, DustID.t_LivingWood);
        }
    }
}